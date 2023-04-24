namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class TransientErrorHandlingConfig
    {
        public int? RestartConsumerAfterMs { get; set; }
        public DeadLetteringConfig? DeadLettering { get; set; }

        public static TransientErrorHandlingConfig Default => new()
        {
            RestartConsumerAfterMs = 5000
        };
    }
}
