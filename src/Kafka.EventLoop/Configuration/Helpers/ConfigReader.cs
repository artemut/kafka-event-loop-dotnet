using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Microsoft.Extensions.Configuration;

namespace Kafka.EventLoop.Configuration.Helpers
{
    internal static class ConfigReader
    {
        public const string KafkaSectionName = "Kafka";

        public static KafkaConfig Read(IConfiguration configuration)
        {
            var section = configuration.GetSection(KafkaSectionName);
            var kafkaConfig = section.Get<KafkaConfig?>();
            
            ConfigValidator.Validate(kafkaConfig);

            InitializeIntakeStrategies(kafkaConfig!, configuration);

            return kafkaConfig!;
        }

        private static void InitializeIntakeStrategies(KafkaConfig kafkaConfig, IConfiguration configuration)
        {
            for (var i = 0; i < kafkaConfig.ConsumerGroups.Length; i++)
            {
                var consumerGroup = kafkaConfig.ConsumerGroups[i];
                var groupId = consumerGroup.GroupId;
                
                if (consumerGroup.Intake?.Strategy == null)
                    continue;

                var strategyName = consumerGroup.Intake.Strategy.Name;

                var key = $"{KafkaSectionName}:" +
                          $"{nameof(KafkaConfig.ConsumerGroups)}:" +
                          $"{i}:" +
                          $"{nameof(ConsumerGroupConfig.Intake)}:" +
                          $"{nameof(IntakeConfig.Strategy)}";
                var section = configuration.GetSection(key);

                consumerGroup.Intake.Strategy = strategyName switch
                {
                    DefaultIntakeStrategyNames.FixedSize => section.Get<FixedSizeIntakeStrategyConfig>(),
                    DefaultIntakeStrategyNames.FixedInterval => section.Get<FixedIntervalIntakeStrategyConfig>(),
                    DefaultIntakeStrategyNames.MaxSizeWithTimeout => section.Get<MaxSizeWithTimeoutIntakeStrategyConfig>(),
                    _ => consumerGroup.Intake.Strategy
                };

                ConfigValidator.Validate(consumerGroup.Intake.Strategy, groupId);
            }
        }
    }
}
