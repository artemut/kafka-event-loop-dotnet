namespace Kafka.EventLoop.Configuration.Options
{
    internal record ConsumerGroupOptions(string GroupId) : IConsumerGroupOptions;
}
