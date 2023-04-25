using Confluent.Kafka;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Conversion;
using Kafka.EventLoop.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.DependencyInjection
{
    /// <summary>
    /// The purpose of this class is to register the same types separately for each of the consumer group.
    /// Default .NET IoC-container doesn't allow keyed/named registrations unfortunately.
    /// </summary>
    internal sealed class DependencyRegistrar : IDependencyRegistrar
    {
        private readonly IServiceCollection _externalRegistry;
        private readonly DependencyRegistry _internalRegistry;

        public DependencyRegistrar(
            IServiceCollection externalRegistry,
            DependencyRegistry internalRegistry)
        {
            _externalRegistry = externalRegistry;
            _internalRegistry = internalRegistry;
        }

        public void AddJsonMessageDeserializer<TMessage>(string consumerGroupName)
        {
            _internalRegistry
                .MessageDeserializerProviders[consumerGroupName] = _ => new JsonDeserializer<TMessage>();
        }

        public void AddCustomMessageDeserializer<TDeserializer>(string consumerGroupName) where TDeserializer : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TDeserializer)))
            {
                _externalRegistry.AddTransient<TDeserializer>();
            }
            _internalRegistry
                .MessageDeserializerProviders[consumerGroupName] = sp => sp.GetRequiredService<TDeserializer>();
        }

        public void AddKafkaController<TController>(string consumerGroupName) where TController : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TController)))
            {
                _externalRegistry.AddScoped<TController>();
            }
            _internalRegistry
                .KafkaControllerProviders[consumerGroupName] = sp => sp.GetRequiredService<TController>();
        }

        public void AddKafkaConsumer<TMessage>(string consumerGroupName)
        {
            _internalRegistry.KafkaConsumerFactories[consumerGroupName] = sp =>
                new KafkaConsumer<TMessage>(
                    (IDeserializer<TMessage>)_internalRegistry.MessageDeserializerProviders[consumerGroupName](sp),
                    sp.GetRequiredService<ILogger<KafkaConsumer<TMessage>>>());
        }

        public void AddIntakeScope<TMessage>(string consumerGroupName)
        {
            _internalRegistry.IntakeScopeFactories[consumerGroupName] = sp =>
                new IntakeScope<TMessage>(
                    sp.GetRequiredService<IServiceScopeFactory>().CreateScope(),
                    scopedSp => (IKafkaController<TMessage>)_internalRegistry.KafkaControllerProviders[consumerGroupName](scopedSp));
        }

        public void AddKafkaWorker<TMessage>(string consumerGroupName)
        {
            _internalRegistry.KafkaWorkerFactories[consumerGroupName] = (sp, consumerId) =>
                new KafkaWorker<TMessage>(
                    consumerGroupName,
                    consumerId,
                    () => (IKafkaConsumer<TMessage>)_internalRegistry.KafkaConsumerFactories[consumerGroupName](sp),
                    () => (IntakeScope<TMessage>)_internalRegistry.IntakeScopeFactories[consumerGroupName](sp));
        }
    }
}
