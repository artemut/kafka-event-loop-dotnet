using Kafka.EventLoop.Consume;
using Kafka.EventLoop.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly string _groupId;
        private readonly int _consumerId;
        private readonly Func<IKafkaConsumer<TMessage>> _kafkaConsumerFactory;
        private readonly Func<IIntakeScope<TMessage>> _intakeScopeFactory;
        private readonly ILogger<KafkaWorker<TMessage>> _logger;
        private int _isRunning;

        public KafkaWorker(
            string groupId,
            int consumerId,
            Func<IKafkaConsumer<TMessage>> kafkaConsumerFactory,
            Func<IIntakeScope<TMessage>> intakeScopeFactory,
            ILogger<KafkaWorker<TMessage>> logger)
        {
            _groupId = groupId;
            _consumerId = consumerId;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _intakeScopeFactory = intakeScopeFactory;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                throw new InvalidOperationException(
                    $"Consumer {_groupId}:{_consumerId} is already running");
            }

            // todo: error handling
            await RunNewConsumerAsync(cancellationToken);
            
            _isRunning = 0;
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

                    var messages = consumer.CollectMessages(intakeStrategy, cancellationToken);
                    if (!messages.Any())
                    {
                        continue;
                    }

                    var controller = intakeScope.GetController();
                    await controller.ProcessAsync(messages, cancellationToken);

                    await consumer.CommitAsync(messages, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while running consumer {_groupId}:{_consumerId}");
                // todo: throw;
            }
            finally
            {
                await consumer.CloseAsync(cancellationToken);
            }
        }
    }
}
