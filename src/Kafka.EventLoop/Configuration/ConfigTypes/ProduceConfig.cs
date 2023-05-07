namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    public class ProduceConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string TopicName { get; set; } = null!;
        public ProduceAckLevel? AckLevel { get; set; }
        public int? RequestTimeoutMs { get; set; }
        public int? SocketTimeoutMs { get; set; }
        public int? MessageTimeoutMs { get; set; }
    }
}
