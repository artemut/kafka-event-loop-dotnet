namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    public enum DeadLetteringFailStrategy
    {
        StopConsumer,
        RestartConsumer
    }
}
