using Kafka.EventLoop.Consume;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IKafkaConsumer<TMessage> Create<TMessage>()
        {
            var consumerType = TypeResolver.BuildConsumerServiceType(typeof(TMessage));
            if (_serviceProvider.GetRequiredService(consumerType) is not IKafkaConsumer<TMessage> consumer)
            {
                throw new InvalidOperationException($"Cannot resolve consumer of type {consumerType.Name}");
            }
            return consumer;
        }
    }
}
