namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    public class ProduceConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string TopicName { get; set; } = null!;
    }
}
