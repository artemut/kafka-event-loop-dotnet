using Confluent.Kafka;

namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ConsumerGroupConfig
    {
        public string GroupId { get; set; }
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Latest;
        public int ParallelConsumers { get; set; }
        public IntakeConfig Intake { get; set; }
        public ErrorHandlingConfig ErrorHandling { get; set; } = ErrorHandlingConfig.Default;
    }
}
