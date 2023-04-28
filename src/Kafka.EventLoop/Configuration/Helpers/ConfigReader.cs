using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Exceptions;
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
                var groupId = consumerGroup.GroupId;
                
                if (consumerGroup.Intake.Strategy == null)
                    continue;

                var strategyName = consumerGroup.Intake.Strategy.Name;

                var key = $"{KafkaSectionName}:" +
                          $"{nameof(KafkaConfig.ConsumerGroups)}:" +
                          $"{i}:" +
                          $"{nameof(ConsumerGroupConfig.Intake)}:" +
                          $"{nameof(IntakeConfig.Strategy)}";
                var section = configuration.GetSection(key);

                try
                {
                    consumerGroup.Intake.Strategy = strategyName switch
                    {
                        DefaultIntakeStrategyNames.FixedSize => section.Get<FixedSizeIntakeStrategyConfig>().AsValid(),
                        DefaultIntakeStrategyNames.FixedInterval => section.Get<FixedIntervalIntakeStrategyConfig>().AsValid(),
                        DefaultIntakeStrategyNames.MaxSizeWithTimeout => section.Get<MaxSizeWithTimeoutIntakeStrategyConfig>().AsValid(),
                        _ => consumerGroup.Intake.Strategy
                    };
                }
                catch (ConfigValidationException ex)
                {
                    var readableKey = key.Replace($":{i}:", $":{groupId}:");
                    throw new ConfigValidationException($"{readableKey}:{ex.PropertyName}", ex.Message);
                }
            }
        }
    }
}
