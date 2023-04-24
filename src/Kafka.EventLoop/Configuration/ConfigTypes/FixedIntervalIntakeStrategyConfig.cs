namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class FixedIntervalIntakeStrategyConfig : IntakeStrategyConfig
    {
        public int IntervalInMs { get; set; }
    }
}
