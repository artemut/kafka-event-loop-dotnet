using Kafka.EventLoop.Configuration.ConfigTypes;
using Microsoft.Extensions.Configuration;

namespace Kafka.EventLoop.Configuration.Helpers
{
    internal static class ConfigReader
    {
        private const string KafkaSectionName = "Kafka";

        public static KafkaConfig Read(IConfiguration configuration)
        {
            // todo: add validation

            var section = configuration.GetSection(KafkaSectionName);
            var kafkaConfig = section.Get<KafkaConfig>();

            InitializeIntakeStrategies(kafkaConfig, configuration);

            return kafkaConfig;
        }

        private static void InitializeIntakeStrategies(KafkaConfig kafkaConfig, IConfiguration configuration)
        {
            for (var i = 0; i < kafkaConfig.ConsumerGroups.Length; i++)
            {
                var consumerGroup = kafkaConfig.ConsumerGroups[i];
                var strategyName = consumerGroup.Intake.Strategy.Name;

                var key = $"{KafkaSectionName}:" +
                          $"{nameof(KafkaConfig.ConsumerGroups)}:" +
                          $"{i}:" +
                          $"{nameof(ConsumerGroupConfig.Intake)}:" +
                          $"{nameof(IntakeConfig.Strategy)}";

                consumerGroup.Intake.Strategy = strategyName switch
                {
                    "FixedSize" => configuration.GetSection(key).Get<FixedSizeIntakeStrategyConfig>(),
                    "FixedInterval" => configuration.GetSection(key).Get<FixedIntervalIntakeStrategyConfig>(),
                    "MaxSizeWithTimeout" => configuration.GetSection(key).Get<MaxSizeWithTimeoutIntakeStrategyConfig>(),
                    _ => consumerGroup.Intake.Strategy
                };
            }
        }
    }
}
