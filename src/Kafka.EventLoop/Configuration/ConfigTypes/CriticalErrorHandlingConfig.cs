namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class CriticalErrorHandlingConfig
    {
        public bool? StopConsumer { get; set; }
        public DeadLetteringConfig? DeadLettering { get; set; }

        public static CriticalErrorHandlingConfig Default => new ()
        {
            StopConsumer = true
        };
    }
}
