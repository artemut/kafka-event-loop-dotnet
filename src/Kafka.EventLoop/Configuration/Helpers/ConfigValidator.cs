using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.Helpers
{
    // ReSharper disable MemberCanBePrivate.Global
    internal static class ConfigValidator
    {
        private const string KafkaSectionName = ConfigReader.KafkaSectionName;

        public static void Validate(KafkaConfig? config)
        {
            if (config == null)
            {
                throw new ConfigValidationException(KafkaSectionName, "Cannot find config section in the settings");
            }
            try
            {
                ValidateConsumerGroups(config.ConsumerGroups);
                ValidateNoDuplicates(config.ConsumerGroups);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException($"{KafkaSectionName}:{ex.PropertyName}", ex.Message);
            }
        }

        public static void ValidateConsumerGroups(ConsumerGroupConfig?[]? configs)
        {
            if (configs == null)
            {
                throw new ConfigValidationException($"{nameof(KafkaConfig.ConsumerGroups)}", "Value is null");
            }
            for (var i = 0; i < configs.Length; i++)
            {
                try
                {
                    Validate(configs[i], i);
                }
                catch (ConfigValidationException ex)
                {
                    throw new ConfigValidationException(
                        $"{nameof(KafkaConfig.ConsumerGroups)}:{ex.PropertyName}", ex.Message);
                }
            }
        }

        public static void Validate(ConsumerGroupConfig? config, int index)
        {
            if (config == null)
            {
                throw new ConfigValidationException(
                    $"{index}", "One of the consumer groups is null");
            }
            if (string.IsNullOrWhiteSpace(config.GroupId))
            {
                throw new ConfigValidationException(
                    $"{index}:{nameof(config.GroupId)}", "Value must be provided");
            }
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.ConnectionString)}", "Value must be provided");
            }
            if (string.IsNullOrWhiteSpace(config.TopicName))
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.TopicName)}", "Value must be provided");
            }
            if (config.ParallelConsumers <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.ParallelConsumers)}", "Value must be greater than 0");
            }
            try
            {
                Validate(config.Intake);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.Intake)}:{ex.PropertyName}", ex.Message);
            }
            try
            {
                Validate(config.Streaming);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.Streaming)}:{ex.PropertyName}", ex.Message);
            }
            try
            {
                Validate(config.ErrorHandling);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.ErrorHandling)}:{ex.PropertyName}", ex.Message);
            }
            if (config.SubscribeTimeoutMs is <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.SubscribeTimeoutMs)}", "Value must be greater than 0");
            }
            if (config.GetCurrentAssignmentTimeoutMs is <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.GetCurrentAssignmentTimeoutMs)}", "Value must be greater than 0");
            }
            if (config.CommitTimeoutMs is <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.CommitTimeoutMs)}", "Value must be greater than 0");
            }
            if (config.SeekTimeoutMs is <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.SeekTimeoutMs)}", "Value must be greater than 0");
            }
            if (config.CloseTimeoutMs is <= 0)
            {
                throw new ConfigValidationException(
                    $"{config.GroupId}:{nameof(config.CloseTimeoutMs)}", "Value must be greater than 0");
            }
        }

        public static void Validate(IntakeConfig? config)
        {
            if (config?.MaxSpeed is <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.MaxSpeed), "Value must be greater than 0");
            }
        }

        public static void Validate(IntakeStrategyConfig intakeStrategy, string groupId)
        {
            try
            {
                switch (intakeStrategy)
                {
                    case FixedSizeIntakeStrategyConfig config:
                        Validate(config);
                        break;
                    case FixedIntervalIntakeStrategyConfig config:
                        Validate(config);
                        break;
                    case MaxSizeWithTimeoutIntakeStrategyConfig config:
                        Validate(config);
                        break;
                }
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{KafkaSectionName}:" +
                    $"{nameof(KafkaConfig.ConsumerGroups)}:" +
                    $"{groupId}:" +
                    $"{nameof(ConsumerGroupConfig.Intake)}:" +
                    $"{nameof(IntakeConfig.Strategy)}:" +
                    $"{ex.PropertyName}",
                    ex.Message);
            }
        }

        public static void Validate(FixedSizeIntakeStrategyConfig config)
        {
            if (config.Size <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.Size), "Value must be greater than 0");
            }
        }

        public static void Validate(FixedIntervalIntakeStrategyConfig config)
        {
            if (config.IntervalInMs <= 0)
            {
                throw new ConfigValidationException(
                    nameof(config.IntervalInMs), "Value must be greater than 0");
            }
        }

        public static void Validate(MaxSizeWithTimeoutIntakeStrategyConfig config)
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
        }

        private static void Validate(ErrorHandlingConfig? config)
        {
            if (config == null)
                return;
            try
            {
                Validate(config.Transient);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{nameof(config.Transient)}:{ex.PropertyName}", ex.Message);
            }
            try
            {
                Validate(config.Critical);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{nameof(config.Critical)}:{ex.PropertyName}", ex.Message);
            }
        }

        private static void Validate(TransientErrorHandlingConfig? config)
        {
            if (config == null)
                return;
            if (config.RestartConsumerAfterMs is not > 0)
            {
                throw new ConfigValidationException(
                    nameof(config.RestartConsumerAfterMs), "Value must be greater than 0");
            }
        }

        private static void Validate(CriticalErrorHandlingConfig? config)
        {
            if (config == null)
                return;
            if (config.StopConsumer.HasValue && config.DeadLettering != null)
            {
                throw new ConfigValidationException(
                    "", $"Please specify either {nameof(config.StopConsumer)} " +
                        $"or {nameof(config.DeadLettering)}");
            }
            try
            {
                Validate(config.DeadLettering);
            }
            catch (ConfigValidationException ex)
            {
                throw new ConfigValidationException(
                    $"{nameof(config.DeadLettering)}:{ex.PropertyName}", ex.Message);
            }
        }

        private static void Validate(ProduceConfig? config)
        {
            if (config == null)
                return;
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                throw new ConfigValidationException(
                    nameof(config.ConnectionString), "Value must be provided");
            }
            if (string.IsNullOrWhiteSpace(config.TopicName))
            {
                throw new ConfigValidationException(
                    nameof(config.TopicName), "Value must be provided");
            }
        }

        private static void ValidateNoDuplicates(ConsumerGroupConfig[] consumerGroups)
        {
            var duplicateGroupIds = consumerGroups
                .GroupBy(x => x.GroupId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();
            if (duplicateGroupIds.Any())
            {
                throw new ConfigValidationException(
                    nameof(KafkaConfig.ConsumerGroups),
                    $"Please provide unique {nameof(ConsumerGroupConfig.GroupId)} value " +
                    $"for each consumer group. Duplicates: {string.Join(",", duplicateGroupIds)}");
            }
        }
    }
}
