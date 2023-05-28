namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class CriticalErrorHandlingConfig
    {
        public DeadLetteringConfig? DeadLettering { get; set; }
    }
}
