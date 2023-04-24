using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.DependencyInjection
{
    internal class KafkaWorkerFactory : IKafkaWorkerFactory
    {
        private readonly IKafkaOptions _options;
        private readonly IIntakeScopeFactory _intakeScopeFactory;

        public KafkaWorkerFactory(IKafkaOptions options, IIntakeScopeFactory intakeScopeFactory)
        {
            _options = options;
            _intakeScopeFactory = intakeScopeFactory;
        }

        public IKafkaWorker Create(string consumerGroupName, int consumerId)
        {
            var consumerGroup = _options.ConsumerGroups.SingleOrDefault(x => x.Name == consumerGroupName);
            if (consumerGroup == null)
            {
                throw new InvalidOperationException(
                    $"Cannot create kafka worker for unknown consumer group {consumerGroupName}");
            }

            var workerType = TypeResolver.BuildWorkerType(consumerGroup.MessageType);
            var args = new object[]
            {
                consumerGroupName,
                consumerId,
                _intakeScopeFactory
            };
            var worker = (IKafkaWorker)Activator.CreateInstance(workerType, args)!;
            return worker;
        }
    }
}
