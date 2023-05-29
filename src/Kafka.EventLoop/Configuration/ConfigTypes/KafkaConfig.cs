namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class KafkaConfig
    {
        public string ConnectionString { get; set; } = null!;
        public ConsumerGroupConfig[] ConsumerGroups { get; set; } = null!;
    }
}
