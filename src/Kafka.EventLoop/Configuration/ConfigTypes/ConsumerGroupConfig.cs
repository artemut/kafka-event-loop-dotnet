namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ConsumerGroupConfig
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public int ParallelConsumers { get; set; }
        public IntakeConfig Intake { get; set; }
        public ErrorHandlingConfig ErrorHandling { get; set; } = ErrorHandlingConfig.Default;
    }
}
