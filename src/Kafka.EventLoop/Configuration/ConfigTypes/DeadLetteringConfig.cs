namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class DeadLetteringConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string TopicName { get; set; } = null!;
    }
}
