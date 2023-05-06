using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class KafkaOptionsBuilder : IKafkaOptionsBuilder
    {
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly KafkaConfig _kafkaConfig;
        private readonly Dictionary<string, IConsumerGroupOptions> _consumerGroups = new();

        public KafkaOptionsBuilder(IDependencyRegistrar dependencyRegistrar, KafkaConfig kafkaConfig)
        {
            _dependencyRegistrar = dependencyRegistrar;
            _kafkaConfig = kafkaConfig;
        }

        public IKafkaOptionsBuilder HasConsumerGroup(
            string groupId,
            Func<IConsumerGroupOptionsBuilder, IConsumerGroupOptions> optionsAction)
        {
            if (_consumerGroups.ContainsKey(groupId))
            {
                throw new InvalidOptionsException(
                    $"Options for consumer group {groupId} are already provided");
            }

            var consumerGroupConfig = _kafkaConfig.ConsumerGroups.SingleOrDefault(x => x.GroupId == groupId);
            if (consumerGroupConfig == null)
            {
                throw new InvalidOptionsException(
                    $"No settings found for consumer group {groupId}");
            }

            var optionsBuilder = new ConsumerGroupOptionsBuilder(groupId, _dependencyRegistrar, consumerGroupConfig);
            var options = optionsAction(optionsBuilder);
            _consumerGroups.Add(groupId, options);

            return this;
        }

        public IKafkaOptions Build()
        {
            var missingOptions = _kafkaConfig
                .ConsumerGroups
                .Where(c => !_consumerGroups.ContainsKey(c.GroupId))
                .Select(c => c.GroupId)
                .ToArray();
            if (missingOptions.Any())
            {
                throw new InvalidOptionsException(
                    missingOptions.Length == 1
                        ? $"Consumer group {missingOptions[0]} is present in " +
                          "the settings but no options are provided for it"
                        : $"Consumer groups {string.Join(", ", missingOptions)} are present " +
                          "in the settings but no options are provided for them");
            }

            return new KafkaOptions(_consumerGroups.Values.ToArray());
        }
    }
}
