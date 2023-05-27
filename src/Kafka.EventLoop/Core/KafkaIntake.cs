using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Exceptions;
using Kafka.EventLoop.Produce;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaIntake<TMessage> : IKafkaIntake
    {
        private readonly IKafkaConsumer<TMessage> _consumer;
        private readonly KafkaIntakeObserver<TMessage>? _intakeObserver;
        private readonly IKafkaIntakeStrategy<TMessage> _intakeStrategy;
        private readonly IKafkaIntakeThrottle _intakeThrottle;
        private readonly Func<IKafkaController<TMessage>> _controllerProvider;
        private readonly Func<IKafkaProducer<TMessage>> _kafkaDeadLetterProducerProvider;
        private readonly ErrorHandlingConfig? _errorHandlingConfig;
        private readonly ILogger<KafkaIntake<TMessage>> _logger;

        public KafkaIntake(
            IKafkaConsumer<TMessage> consumer,
            Func<KafkaIntakeObserver<TMessage>?> intakeObserverFactory,
            Func<IKafkaIntakeStrategy<TMessage>> intakeStrategyFactory,
            Func<IKafkaIntakeThrottle> intakeThrottleProvider,
            Func<IKafkaController<TMessage>> controllerProvider,
            Func<IKafkaProducer<TMessage>> kafkaDeadLetterProducerProvider,
            ErrorHandlingConfig? errorHandlingConfig,
            ILogger<KafkaIntake<TMessage>> logger)
        {
            _consumer = consumer;
            _intakeObserver = intakeObserverFactory();
            _intakeStrategy = intakeStrategyFactory();
            _intakeThrottle = intakeThrottleProvider();
            _controllerProvider = controllerProvider;
            _kafkaDeadLetterProducerProvider = kafkaDeadLetterProducerProvider;
            _errorHandlingConfig = errorHandlingConfig;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _intakeThrottle.ControlSpeedAsync(
                async () => await ExecuteInternalAsync(cancellationToken),
                cancellationToken);
        }

        public void Dispose()
        {
            _intakeObserver?.Dispose();
        }

        private async Task<ThrottleOptions> ExecuteInternalAsync(CancellationToken cancellationToken)
        {
            bool containsPartialResults;
            MessageInfo<TMessage>[] messages;
            try
            {
                messages = _consumer.CollectMessages(
                    _intakeStrategy,
                    out containsPartialResults,
                    cancellationToken);
            }
            catch (ConnectivityException ex)
            {
                _intakeObserver?.OnConsumeError(ex.Error);
                throw;
            }

            if (!messages.Any())
            {
                _intakeObserver?.OnNothingToProcess();
                return ThrottleOptions.Empty;
            }
            _intakeObserver?.OnMessagesCollected(messages);

            List<TopicPartition> assignment;
            try
            {
                assignment = await _consumer.GetCurrentAssignmentAsync(cancellationToken);
            }
            catch (ConnectivityException ex)
            {
                _intakeObserver?.OnConsumeError(ex.Error);
                throw;
            }
            
            await ProcessMessagesAsync(messages, cancellationToken);

            try
            {
                await _consumer.CommitAsync(messages, cancellationToken);
            }
            catch (ConnectivityException ex)
            {
                _intakeObserver?.OnCommitError(ex.Error);
                throw;
            }

            if (containsPartialResults)
            {
                // some messages were consumed from kafka but not processed
                // we need to seek consumer back
                // so that those messages are consumed again in the next iteration
                try
                {
                    await _consumer.SeekAsync(messages, cancellationToken);
                }
                catch (ConnectivityException ex)
                {
                    _intakeObserver?.OnCommitError(ex.Error);
                    throw;
                }
            }

            _intakeObserver?.OnCommitted();

            return new ThrottleOptions(assignment.Count, messages.Length);
        }

        private async Task ProcessMessagesAsync(
            MessageInfo<TMessage>[] messages,
            CancellationToken cancellationToken)
        {
            var controller = _controllerProvider();
            try
            {
                await controller.ProcessAsync(messages, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TransientException ex)
            {
                _intakeObserver?.OnProcessingException(ex);
                throw;
            }
            catch (Exception ex)
            {
                _intakeObserver?.OnProcessingException(ex);
                var deadLetteringConfig = _errorHandlingConfig?.Critical?.DeadLettering;
                if (deadLetteringConfig == null)
                    throw;

                await SendDeadLetterMessagesAsync(
                    messages,
                    deadLetteringConfig,
                    ex,
                    cancellationToken);
                return;
            }
            _intakeObserver?.OnProcessingFinished();
        }

        private async Task SendDeadLetterMessagesAsync(
            MessageInfo<TMessage>[] deadLetters,
            DeadLetteringConfig deadLetteringConfig,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _intakeObserver?.OnDeadLettering();
            try
            {
                _logger.LogCritical(exception, "Critical error while processing messages in consumer {ConsumerId}", _consumer.ConsumerId);
                _logger.LogWarning(
                    "Sending {MessageCount} message(s) to the dead-letter topic for consumer {ConsumerId}",
                    deadLetters.Length, _consumer.ConsumerId);

                var producer = _kafkaDeadLetterProducerProvider();
                await producer.SendMessagesAsync(
                    deadLetters.Select(m => m.Value).ToArray(),
                    deadLetteringConfig.SendSequentially,
                    null,
                    cancellationToken);

                _logger.LogInformation(
                    "Successfully sent {MessageCount} message(s) to the dead-letter topic for consumer {ConsumerId}",
                    deadLetters.Length, _consumer.ConsumerId);
            }
            catch (ProduceException ex)
            {
                _intakeObserver?.OnDeadLetteringFailed(ex);
                throw new DeadLetteringFailedException(deadLetteringConfig.OnDeadLetteringFailed, ex);
            }
            _intakeObserver?.OnDeadLetteringFinished();
        }
    }
}
