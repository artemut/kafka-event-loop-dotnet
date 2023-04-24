using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IKafkaWorkerFactory
    {
        IKafkaWorker Create(string consumerGroupName, int consumerId);
    }
}
