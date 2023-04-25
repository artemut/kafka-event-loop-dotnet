using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder : IConsumerGroupOptionsBuilder
    {
        private readonly string _groupId;
        private readonly IDependencyRegistrar _dependencyRegistrar;

        public ConsumerGroupOptionsBuilder(string groupId, IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _dependencyRegistrar = dependencyRegistrar;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasMessageType<TMessage>()
        {
            return new ConsumerGroupOptionsBuilder<TMessage>(_groupId, _dependencyRegistrar);
        }
    }
}
