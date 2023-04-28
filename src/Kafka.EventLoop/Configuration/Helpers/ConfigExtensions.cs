using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Exceptions;

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

        public static FixedSizeIntakeStrategyConfig AsValid(this FixedSizeIntakeStrategyConfig config)
        {
            if (config.Size <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.Size), "Value must be greater than 0");
            }
            return config;
        }

        public static FixedIntervalIntakeStrategyConfig AsValid(this FixedIntervalIntakeStrategyConfig config)
        {
            if (config.IntervalInMs <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.IntervalInMs), "Value must be greater than 0");
            }
            return config;
        }

        public static MaxSizeWithTimeoutIntakeStrategyConfig AsValid(this MaxSizeWithTimeoutIntakeStrategyConfig config)
        {
            if (config.MaxSize <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.MaxSize), "Value must be greater than 0");
            }
            if (config.TimeoutInMs <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.TimeoutInMs), "Value must be greater than 0");
            }
            return config;
        }
    }
}
