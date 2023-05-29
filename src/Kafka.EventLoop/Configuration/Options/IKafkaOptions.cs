namespace Kafka.EventLoop.Configuration.Options
{
    public interface IKafkaOptions
    {
        IConsumerGroupOptions[] ConsumerGroups { get; }
    }
}
