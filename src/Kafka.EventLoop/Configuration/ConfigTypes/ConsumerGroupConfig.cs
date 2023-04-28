using Confluent.Kafka;

namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ConsumerGroupConfig
    {
        public string GroupId { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public string TopicName { get; set; } = null!;
        public AutoOffsetReset? AutoOffsetReset { get; set; }
        public int ParallelConsumers { get; set; }
        public IntakeConfig? Intake { get; set; }
        public ErrorHandlingConfig? ErrorHandling { get; set; }
    }
}
