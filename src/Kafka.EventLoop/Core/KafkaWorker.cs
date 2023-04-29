using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Exceptions;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private const int DefaultRestartDelayInMs = 5000;

        private readonly string _consumerName;
        private readonly ErrorHandlingConfig? _errorHandlingConfig;
        private readonly Func<IKafkaConsumer<TMessage>> _kafkaConsumerFactory;
        private readonly Func<IIntakeScope<TMessage>> _intakeScopeFactory;
        private readonly ILogger<KafkaWorker<TMessage>> _logger;
        private int _isRunning;

        public KafkaWorker(
            string groupId,
            int consumerId,
            ErrorHandlingConfig? errorHandlingConfig,
            Func<IKafkaConsumer<TMessage>> kafkaConsumerFactory,
            Func<IIntakeScope<TMessage>> intakeScopeFactory,
            ILogger<KafkaWorker<TMessage>> logger)
        {
            _consumerName = $"{groupId}:{consumerId}";
            _errorHandlingConfig = errorHandlingConfig;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _intakeScopeFactory = intakeScopeFactory;
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
                ProcessingException<TMessage>? processingException = null;
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
                catch (ProcessingException<TMessage> ex)
                {
                    _logger.Log(
                        ex.ErrorCode == ProcessingErrorCode.CriticalError ? LogLevel.Critical : LogLevel.Error,
                        ex.InnerException,
                        "Error while processing messages in consumer {ConsumerName} [{ErrorCode}]",
                        _consumerName, ex.ErrorCode);
                    isRetryable = ex.ErrorCode == ProcessingErrorCode.TransientError;
                    processingException = ex;
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
                    var delay = _errorHandlingConfig?.Transient?.RestartConsumerAfterMs ?? DefaultRestartDelayInMs;
                    _logger.LogWarning("Restarting consumer {ConsumerName} in {Delay} ms...", _consumerName, delay);
                    await Task.Delay(delay, cancellationToken);
                }
                else if (processingException == null || _errorHandlingConfig?.Critical?.DeadLettering == null)
                {
                    _logger.LogCritical("Consumer {ConsumerName} was stopped", _consumerName);
                    return;
                }
                else
                {
                    // todo: send messages to dead letter topic
                    throw new NotImplementedException();
                }
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
                    using var intakeScope = _intakeScopeFactory();
                    using var intakeStrategy = intakeScope.CreateIntakeStrategy();
                    var intakeThrottle = intakeScope.CreateIntakeThrottle();

                    var messages = consumer.CollectMessages(intakeStrategy, cancellationToken);
                    if (!messages.Any())
                    {
                        continue;
                    }

                    var assignment = await consumer.GetCurrentAssignmentAsync(cancellationToken);

                    await ProcessMessagesAsync(intakeScope, messages, cancellationToken);

                    await consumer.CommitAsync(messages, cancellationToken);

                    var throttleOptions = new ThrottleOptions(assignment.Count, messages.Length);
                    await intakeThrottle.WaitAsync(throttleOptions, cancellationToken);
                }
            }
            finally
            {
                await consumer.CloseAsync(cancellationToken);
            }
        }

        private static async Task ProcessMessagesAsync(
            IIntakeScope<TMessage> intakeScope,
            MessageInfo<TMessage>[] messages,
            CancellationToken cancellationToken)
        {
            var controller = intakeScope.GetController();
            MessageProcessingResult result;
            try
            {
                result = await controller.ProcessAsync(messages, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ProcessingException<TMessage>(ProcessingErrorCode.CriticalError, messages, ex);
            }

            switch (result)
            {
                case MessageProcessingResult.Success:
                    return;
                case MessageProcessingResult.TransientError:
                    throw new ProcessingException<TMessage>(ProcessingErrorCode.TransientError, messages);
                case MessageProcessingResult.CriticalError:
                    throw new ProcessingException<TMessage>(ProcessingErrorCode.CriticalError, messages);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(result), result, "Unknown message processing result");
            }
        }
    }
}
