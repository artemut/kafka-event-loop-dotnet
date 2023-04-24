namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class IntakeConfig
    {
        public int? MaxSpeed { get; set; }
        public IntakeStrategyConfig Strategy { get; set; }
    }
}
