using Kafka.EventLoop.Consume;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IKafkaConsumerFactory
    {
        IKafkaConsumer<TMessage> Create<TMessage>();
    }
}
