using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder : IConsumerGroupOptionsBuilder
    {
        private readonly string _name;
        private readonly IDependencyRegistrar _dependencyRegistrar;

        public ConsumerGroupOptionsBuilder(string name, IDependencyRegistrar dependencyRegistrar)
        {
            _name = name;
            _dependencyRegistrar = dependencyRegistrar;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasMessageType<TMessage>()
        {
            return new ConsumerGroupOptionsBuilder<TMessage>(_name, _dependencyRegistrar);
        }
    }
}
