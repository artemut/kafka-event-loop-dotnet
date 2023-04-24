namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class MaxSizeWithTimeoutIntakeStrategyConfig : IntakeStrategyConfig
    {
        public int MaxSize { get; set; }
        public int TimeoutInMs { get; set; }
    }
}
