namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal static class DefaultIntakeStrategyNames
    {
        public const string FixedSize = "FixedSize";
        public const string FixedInterval = "FixedInterval";
        public const string MaxSizeWithTimeout = "MaxSizeWithTimeout";
    }
}
