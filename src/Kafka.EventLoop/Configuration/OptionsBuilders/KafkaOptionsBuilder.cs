using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class KafkaOptionsBuilder : IKafkaOptionsBuilder
    {
        private readonly Dictionary<string, IConsumerGroupOptions> _consumerGroups = new();

        public IKafkaOptionsBuilder HasConsumerGroup(
            string name,
            Func<IConsumerGroupOptionsBuilder, IConsumerGroupOptions> optionsAction)
        {
            if (_consumerGroups.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Consumer group {name} is already configured");
            }

            var optionsBuilder = new ConsumerGroupOptionsBuilder(name);
            var options = optionsAction(optionsBuilder);
            _consumerGroups.Add(name, options);

            return this;
        }

        public IKafkaOptions Build()
        {
            return new KafkaOptions(_consumerGroups.Values.ToArray());
        }
    }
}
