namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ErrorHandlingConfig
    {
        public int? PauseAfterTransientErrorMs { get; set; }
        public DeadLetteringConfig? DeadLettering { get; set; }
    }
}
