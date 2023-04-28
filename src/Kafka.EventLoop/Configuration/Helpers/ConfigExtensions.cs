using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume.IntakeStrategies;

namespace Kafka.EventLoop.Configuration.Helpers
{
    internal static class ConfigExtensions
    {
        public static bool IsDefault(this IntakeStrategyConfig config)
        {
            return config is
            {
                Name:
                DefaultIntakeStrategyNames.FixedSize or
                DefaultIntakeStrategyNames.FixedInterval or
                DefaultIntakeStrategyNames.MaxSizeWithTimeout
            };
        }
    }
}
