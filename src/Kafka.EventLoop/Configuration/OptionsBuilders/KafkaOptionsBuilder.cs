using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class KafkaOptionsBuilder : IKafkaOptionsBuilder
    {
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly Dictionary<string, IConsumerGroupOptions> _consumerGroups = new();

        public KafkaOptionsBuilder(IDependencyRegistrar dependencyRegistrar)
        {
            _dependencyRegistrar = dependencyRegistrar;
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

            var optionsBuilder = new ConsumerGroupOptionsBuilder(groupId, _dependencyRegistrar);
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
