namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Settings
{
    internal class TestSetupConfig
    {
        public string ConnectionString { get; set; } = null!;
        public TopicConfig[] Topics { get; set; } = null!;
    }
}
