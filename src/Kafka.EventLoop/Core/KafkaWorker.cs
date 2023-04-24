using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly IConsumerGroupOptions _consumerGroupOptions;
        private readonly int _consumerId;
        private readonly IKafkaConsumerFactory _kafkaConsumerFactory;
        private readonly IIntakeScopeFactory _intakeScopeFactory;
        private int _isRunning;

        public KafkaWorker(
            IConsumerGroupOptions consumerGroupOptions,
            int consumerId,
            IKafkaConsumerFactory kafkaConsumerFactory,
            IIntakeScopeFactory intakeScopeFactory)
        {
            _consumerGroupOptions = consumerGroupOptions;
            _consumerId = consumerId;
            _kafkaConsumerFactory = kafkaConsumerFactory;
            _intakeScopeFactory = intakeScopeFactory;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                throw new InvalidOperationException(
                    $"Consumer {_consumerGroupOptions.Name}:{_consumerId} is already running");
            }

            // todo: error handling
            await RunNewConsumerAsync(cancellationToken);

            _isRunning = 0;
        }

        private async Task RunNewConsumerAsync(CancellationToken cancellationToken)
        {
            using var consumer = _kafkaConsumerFactory.Create<TMessage>();
            try
            {
                await consumer.SubscribeAsync(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    using var intakeScope = _intakeScopeFactory.CreateScope<TMessage>(_consumerGroupOptions);

                    var messages = consumer.CollectMessages(cancellationToken);

                    var controller = intakeScope.GetController();
                    await controller.ProcessAsync(messages, cancellationToken);

                    await consumer.CommitAsync(messages, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                await consumer.CloseAsync(cancellationToken);
            }
        }
    }
}
