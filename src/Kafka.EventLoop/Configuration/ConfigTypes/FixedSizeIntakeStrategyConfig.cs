namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class FixedSizeIntakeStrategyConfig : IntakeStrategyConfig
    {
        public int Size { get; set; }
    }
}
