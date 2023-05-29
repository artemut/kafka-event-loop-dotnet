namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Settings
{
    internal class TopicConfig
    {
        public string Name { get; set; } = null!;
        public int NumberOfPartitions { get; set; }
    }
}
