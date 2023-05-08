using Kafka.EventLoop.Configuration;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly string _consumerName;
        private readonly ErrorHandlingConfig? _errorHandlingConfig;
        private readonly Func<IKafkaConsumer<TMessage>> _kafkaConsumerFactory;
        private readonly Func<IKafkaConsumer<TMessage>, IKafkaIntake> _kafkaIntakeFactory;
        private readonly ILogger<KafkaWorker<TMessage>> _logger;
        private int _isRunning;

        public KafkaWorker(
            string consumerName,
            ErrorHandlingConfig? errorHandlingConfig,
            Func<IKafkaConsumer<TMessage>> kafkaConsumerFactory,
            Func<IKafkaConsumer<TMessage>, IKafkaIntake> kafkaIntakeFactory,
            ILogger<KafkaWorker<TMessage>> logger)
        {
            _consumerName = consumerName;
            _errorHandlingConfig = errorHandlingConfig;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _kafkaIntakeFactory = kafkaIntakeFactory;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                throw new InvalidOperationException($"Consumer {_consumerName} is already running");
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
                    _logger.LogError(ex, "Timeout error while running consumer {ConsumerName}", _consumerName);
                    isRetryable = true;
                }
                catch (ConnectivityException ex) when (!ex.IsFatal)
                {
                    _logger.LogError(ex, "Connectivity error while running consumer {ConsumerName}", _consumerName);
                    isRetryable = true;
                }
                catch (ConnectivityException ex)
                {
                    _logger.LogCritical(ex, "Fatal connectivity error while running consumer {ConsumerName}", _consumerName);
                    isRetryable = false;
                }
                catch (ProcessingException ex) when(ex.ErrorCode == ProcessingErrorCode.TransientError)
                {
                    _logger.LogError(ex.InnerException, "Transient error while processing messages in consumer {ConsumerName}", _consumerName);
                    isRetryable = true;
                }
                catch (ProcessingException ex) when (ex.ErrorCode == ProcessingErrorCode.CriticalError)
                {
                    // we catch this exception here when dead-lettering is not enabled
                    _logger.LogCritical(ex.InnerException, "Critical error while processing messages in consumer {ConsumerName}", _consumerName);
                    isRetryable = false;
                }
                catch (DeadLetteringFailedException ex)
                {
                    _logger.LogCritical(ex.InnerException, "Dead-lettering failed for consumer {ConsumerName}", _consumerName);
                    isRetryable = ex.Strategy is DeadLetteringFailStrategy.RestartConsumer or null;
                }
                catch (DependencyException ex)
                {
                    _logger.LogCritical(ex, "Cannot run consumer {ConsumerName} because of the dependency error", _consumerName);
                    isRetryable = false;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Consumer {ConsumerName} was cancelled", _consumerName);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unknown error while running consumer {ConsumerName}", _consumerName);
                    isRetryable = false;
                }

                if (isRetryable)
                {
                    var delay = _errorHandlingConfig?.Transient?.RestartConsumerAfterMs ?? Defaults.RestartConsumerAfterMs;
                    _logger.LogWarning("Restarting consumer {ConsumerName} in {Delay} ms...", _consumerName, delay);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                _logger.LogCritical("Consumer {ConsumerName} was stopped", _consumerName);
                return;
            }
        }

        private async Task RunNewConsumerAsync(CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerFactory();
            try
            {
                await consumer.SubscribeAsync(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    using var intake = _kafkaIntakeFactory(consumer);
                    await intake.ExecuteAsync(cancellationToken);
                }
            }
            finally
            {
                await consumer.CloseAsync(cancellationToken);
            }
        }
    }
}
