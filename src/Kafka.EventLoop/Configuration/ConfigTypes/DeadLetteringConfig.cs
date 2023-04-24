namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class DeadLetteringConfig
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
    }
}
