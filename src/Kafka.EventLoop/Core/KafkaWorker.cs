﻿using Kafka.EventLoop.Configuration;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly ConsumerId _consumerId;
        private readonly Func<IKafkaConsumer<TMessage>> _kafkaConsumerFactory;
        private readonly Func<IKafkaConsumer<TMessage>, IKafkaIntake> _kafkaIntakeFactory;
        private readonly KafkaGlobalObserver? _kafkaGlobalObserver;
        private readonly ILogger<KafkaWorker<TMessage>> _logger;
        private int _isRunning;

        public KafkaWorker(
            ConsumerId consumerId,
            Func<IKafkaConsumer<TMessage>> kafkaConsumerFactory,
            Func<IKafkaConsumer<TMessage>, IKafkaIntake> kafkaIntakeFactory,
            KafkaGlobalObserver? kafkaGlobalObserver,
            ILogger<KafkaWorker<TMessage>> logger)
        {
            _consumerId = consumerId;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _kafkaIntakeFactory = kafkaIntakeFactory;
            _kafkaGlobalObserver = kafkaGlobalObserver;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                throw new InvalidOperationException($"Consumer {_consumerId} is already running");
            }
            
            await RunWithRetries(cancellationToken);

            _isRunning = 0;
        }

        private async Task RunWithRetries(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var isRetryable = false;
                try
                {
                    await RunNewConsumerAsync(cancellationToken);
                }
                catch (TimeoutException ex)
                {
                    _logger.LogError(ex, "Timeout error while running consumer {ConsumerId}", _consumerId);
                    isRetryable = true;
                }
                catch (ConnectivityException ex) when (!ex.Error.IsFatal)
                {
                    _logger.LogError(ex, "Connectivity error while running consumer {ConsumerId}", _consumerId);
                    isRetryable = true;
                }
                catch (ConnectivityException ex)
                {
                    _logger.LogCritical(ex, "Fatal connectivity error while running consumer {ConsumerId}", _consumerId);
                    isRetryable = false;
                }
                catch (DeadLetteringFailedException ex)
                {
                    _logger.LogCritical(ex.InnerException, "Dead-lettering failed for consumer {ConsumerId}", _consumerId);
                    isRetryable = true;
                }
                catch (DependencyException ex)
                {
                    _logger.LogCritical(ex, "Cannot run consumer {ConsumerId} because of the dependency error", _consumerId);
                    isRetryable = false;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Consumer {ConsumerId} was cancelled", _consumerId);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unknown error while running consumer {ConsumerId}", _consumerId);
                    isRetryable = false;
                }

                if (isRetryable)
                {
                    var delay = Defaults.RestartConsumerAfterMs;
                    _logger.LogWarning("Restarting consumer {ConsumerId} in {Delay} ms...", _consumerId, delay);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogCritical("Consumer {ConsumerId} was stopped", _consumerId);
                return;
            }
        }

        private async Task RunNewConsumerAsync(CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerFactory();
            try
            {
                await consumer.SubscribeAsync(cancellationToken);
                _kafkaGlobalObserver?.OnConsumerSubscribed(_consumerId);

                while (!cancellationToken.IsCancellationRequested)
                {
                    using var intake = _kafkaIntakeFactory(consumer);
                    await intake.ExecuteAsync(cancellationToken);
                }
            }
            finally
            {
                await consumer.CloseAsync();
            }
        }
    }
}
