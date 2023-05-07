using Kafka.EventLoop.Configuration;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Exceptions;
using Kafka.EventLoop.Produce;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly string _consumerName;
        private readonly ErrorHandlingConfig? _errorHandlingConfig;
        private readonly Func<IKafkaConsumer<TMessage>> _kafkaConsumerFactory;
        private readonly Func<IIntakeScope<TMessage>> _intakeScopeFactory;
        private readonly Func<IKafkaProducer<TMessage>> _kafkaDeadLetterProducerProvider;
        private readonly ILogger<KafkaWorker<TMessage>> _logger;
        private int _isRunning;

        public KafkaWorker(
            string groupId,
            int consumerId,
            ErrorHandlingConfig? errorHandlingConfig,
            Func<IKafkaConsumer<TMessage>> kafkaConsumerFactory,
            Func<IIntakeScope<TMessage>> intakeScopeFactory,
            Func<IKafkaProducer<TMessage>> kafkaDeadLetterProducerProvider,
            ILogger<KafkaWorker<TMessage>> logger)
        {
            _consumerName = $"{groupId}:{consumerId}";
            _errorHandlingConfig = errorHandlingConfig;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _intakeScopeFactory = intakeScopeFactory;
            _kafkaDeadLetterProducerProvider = kafkaDeadLetterProducerProvider;
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
                    using var intakeScope = _intakeScopeFactory();
                    using var intakeStrategy = intakeScope.CreateIntakeStrategy();
                    var intakeThrottle = intakeScope.GetIntakeThrottle();

                    await intakeThrottle.ControlSpeedAsync(async () =>
                    {
                        var messages = consumer.CollectMessages(intakeStrategy, cancellationToken);
                        if (!messages.Any())
                        {
                            return ThrottleOptions.Empty;
                        }

                        var intakeFilter = intakeScope.GetIntakeFilter();
                        var result = intakeFilter.FilterMessages(messages);

                        var assignment = await consumer.GetCurrentAssignmentAsync(cancellationToken);

                        await ProcessMessagesAsync(intakeScope, result.Messages, cancellationToken);

                        await consumer.CommitAsync(result.Messages, cancellationToken);

                        if (result.PartitionToLastAllowedOffset != null)
                        {
                            // if some offsets weren't committed
                            // we need to seek consumer back
                            // so that filtered out messages are consumed again the next iteration
                            await consumer.SeekAsync(result.PartitionToLastAllowedOffset, cancellationToken);
                        }

                        return new ThrottleOptions(assignment.Count, result.Messages.Length);
                    }, cancellationToken);
                }
            }
            finally
            {
                await consumer.CloseAsync(cancellationToken);
            }
        }

        private async Task ProcessMessagesAsync(
            IIntakeScope<TMessage> intakeScope,
            MessageInfo<TMessage>[] messages,
            CancellationToken cancellationToken)
        {
            var controller = intakeScope.GetController();
            try
            {
                await controller.ProcessAsync(messages, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ProcessingException ex) when(ex.ErrorCode == ProcessingErrorCode.TransientError)
            {
                throw;
            }
            catch (Exception ex)
            {
                var deadLetteringConfig = _errorHandlingConfig?.Critical?.DeadLettering;
                if (deadLetteringConfig == null)
                    throw;

                await SendDeadLetterMessagesAsync(
                    messages,
                    deadLetteringConfig,
                    ex,
                    cancellationToken);
            }
        }

        private async Task SendDeadLetterMessagesAsync(
            MessageInfo<TMessage>[] deadLetters,
            DeadLetteringConfig deadLetteringConfig,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogCritical(exception, "Critical error while processing messages in consumer {ConsumerName}", _consumerName);
                _logger.LogWarning(
                    "Sending {MessageCount} message(s) to the dead-letter topic for consumer {ConsumerName}",
                    deadLetters.Length, _consumerName);

                var producer = _kafkaDeadLetterProducerProvider();
                await producer.SendMessagesAsync(
                    deadLetters.Select(m => m.Value).ToArray(),
                    deadLetteringConfig.SendSequentially,
                    null,
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully sent {MessageCount} message(s) to the dead-letter topic for consumer {ConsumerName}",
                    deadLetters.Length, _consumerName);
            }
            catch (ProduceException ex)
            {
                throw new DeadLetteringFailedException(deadLetteringConfig.OnDeadLetteringFailed, ex);
            }
        }
    }
}
