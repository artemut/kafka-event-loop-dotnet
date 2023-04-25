using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.DependencyInjection;

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
                throw new InvalidOperationException(
                    $"Consumer group {groupId} is already configured");
            }

            var consumerGroupConfig = _kafkaConfig.ConsumerGroups.SingleOrDefault(x => x.GroupId == groupId);
            if (consumerGroupConfig == null)
            {
                throw new InvalidOperationException(
                    $"No configuration found for consumer group {groupId}");
            }

            var optionsBuilder = new ConsumerGroupOptionsBuilder(groupId, _dependencyRegistrar, consumerGroupConfig);
            var options = optionsAction(optionsBuilder);
            _consumerGroups.Add(groupId, options);

            return this;
        }

        public IKafkaOptions Build()
        {
            return new KafkaOptions(_consumerGroups.Values.ToArray());
        }
    }
}
