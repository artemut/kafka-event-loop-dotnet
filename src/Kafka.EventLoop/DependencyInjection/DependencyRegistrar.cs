using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Conversion;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.Utils;
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

        public void AddJsonMessageDeserializer<TMessage>(string groupId)
        {
            _internalRegistry
                .MessageDeserializerProviders[groupId] = _ => new JsonDeserializer<TMessage>();
        }

        public void AddCustomMessageDeserializer<TDeserializer>(string groupId) where TDeserializer : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TDeserializer)))
            {
                _externalRegistry.AddTransient<TDeserializer>();
            }
            _internalRegistry
                .MessageDeserializerProviders[groupId] = sp => sp.GetRequiredService<TDeserializer>();
        }

        public void AddFixedSizeIntakeStrategy<TMessage>(string groupId, FixedSizeIntakeStrategyConfig config)
        {
            _internalRegistry.KafkaIntakeStrategyFactories[groupId] = 
                _ => new FixedSizeIntakeStrategy<TMessage>(config.Size);
        }

        public void AddFixedIntervalIntakeStrategy<TMessage>(string groupId, FixedIntervalIntakeStrategyConfig config)
        {
            _internalRegistry.KafkaIntakeStrategyFactories[groupId] = 
                _ => new FixedIntervalIntakeStrategy<TMessage>(config.IntervalInMs);
        }

        public void AddMaxSizeWithTimeoutIntakeStrategy<TMessage>(string groupId, MaxSizeWithTimeoutIntakeStrategyConfig config)
        {
            _internalRegistry.KafkaIntakeStrategyFactories[groupId] =
                _ => new MaxSizeWithTimeoutIntakeStrategy<TMessage>(config.MaxSize, config.TimeoutInMs);
        }

        public void AddCustomIntakeStrategy<TStrategy>(string groupId) where TStrategy : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TStrategy)))
            {
                _externalRegistry.AddScoped<TStrategy>();
            }
            _internalRegistry
                .KafkaIntakeStrategyFactories[groupId] = sp => sp.GetRequiredService<TStrategy>();
        }

        public void AddKafkaController<TController>(string groupId) where TController : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TController)))
            {
                _externalRegistry.AddScoped<TController>();
            }
            _internalRegistry
                .KafkaControllerProviders[groupId] = sp => sp.GetRequiredService<TController>();
        }

        public void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config)
        {
            _internalRegistry.ConsumerGroupConfigProviders[groupId] = config;
        }

        public void AddConfluentConsumerConfig(string groupId, ConsumerConfig config)
        {
            _internalRegistry.ConfluentConsumerConfigProviders[groupId] = config;
        }

        public void AddKafkaConsumer<TMessage>(string groupId)
        {
            _internalRegistry.KafkaConsumerFactories[groupId] = sp =>
                new KafkaConsumer<TMessage>(
                    BuildConfluentConsumer<TMessage>(groupId, sp),
                    _internalRegistry.ConsumerGroupConfigProviders[groupId],
                    new TimeoutRunner());
        }

        public void AddIntakeScope<TMessage>(string groupId)
        {
            _internalRegistry.IntakeScopeFactories[groupId] = sp =>
                new IntakeScope<TMessage>(
                    sp.GetRequiredService<IServiceScopeFactory>().CreateScope(),
                    scopedSp => (IKafkaIntakeStrategy<TMessage>)_internalRegistry.KafkaIntakeStrategyFactories[groupId](scopedSp),
                    scopedSp => (IKafkaController<TMessage>)_internalRegistry.KafkaControllerProviders[groupId](scopedSp));
        }

        public void AddKafkaWorker<TMessage>(string groupId)
        {
            _internalRegistry.KafkaWorkerFactories[groupId] = (sp, consumerId) =>
                new KafkaWorker<TMessage>(
                    groupId,
                    consumerId,
                    () => (IKafkaConsumer<TMessage>)_internalRegistry.KafkaConsumerFactories[groupId](sp),
                    () => (IntakeScope<TMessage>)_internalRegistry.IntakeScopeFactories[groupId](sp),
                    sp.GetRequiredService<ILogger<KafkaWorker<TMessage>>>());
        }

        private IConsumer<Ignore, TMessage> BuildConfluentConsumer<TMessage>(
            string groupId,
            IServiceProvider serviceProvider)
        {
            var config = _internalRegistry.ConfluentConsumerConfigProviders[groupId];
            var builder = new ConsumerBuilder<Ignore, TMessage>(config);
            var deserializer = (IDeserializer<TMessage>)_internalRegistry
                .MessageDeserializerProviders[groupId](serviceProvider);
            return builder
                .SetValueDeserializer(deserializer)
                .SetLogHandler((_, _) => { }) // todo: consider customization
                .Build();
        }
    }
}
