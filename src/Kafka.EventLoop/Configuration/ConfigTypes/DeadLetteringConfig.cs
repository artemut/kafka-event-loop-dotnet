namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class DeadLetteringConfig : ProduceConfig
    {
        public bool SendSequentially { get; set; }
        public DeadLetteringFailStrategy? OnDeadLetteringFailed { get; set; }
    }
}
