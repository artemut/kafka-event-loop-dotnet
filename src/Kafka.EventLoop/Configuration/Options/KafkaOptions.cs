namespace Kafka.EventLoop.Configuration.Options
{
    internal record KafkaOptions(
        IConsumerGroupOptions[] ConsumerGroups) : IKafkaOptions;
}
