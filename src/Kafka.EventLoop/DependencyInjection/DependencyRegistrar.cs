using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Consume.Throttling;
using Kafka.EventLoop.Conversion;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.Produce;
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
            _internalRegistry.MessageDeserializerProviders[groupId] = 
                sp => sp.GetOrThrow<TDeserializer>(
                    $"Error in custom message deserialization for consumer group {groupId}");
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
            _internalRegistry.KafkaIntakeStrategyFactories[groupId] = 
                sp => sp.GetOrThrow<TStrategy>(
                    $"Error in custom intake strategy for consumer group {groupId}");
        }

        public void AddDefaultIntakeThrottle(string groupId, IntakeConfig? intakeConfig)
        {
            var singleInstance = new DefaultKafkaIntakeThrottle(
                intakeConfig?.MaxSpeed,
                () => new StopwatchAdapter(),
                Task.Delay);
            _internalRegistry.KafkaIntakeThrottleProviders[groupId] = _ => singleInstance;
        }

        public void AddCustomIntakeThrottle<TThrottle>(string groupId) where TThrottle : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TThrottle)))
            {
                _externalRegistry.AddScoped<TThrottle>();
            }
            _internalRegistry.KafkaIntakeThrottleProviders[groupId] = 
                sp => sp.GetOrThrow<TThrottle>(
                    $"Error in custom intake throttle for consumer group {groupId}");
        }

        public void AddKafkaController<TController>(string groupId) where TController : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TController)))
            {
                _externalRegistry.AddScoped<TController>();
            }
            _internalRegistry.KafkaControllerProviders[groupId] = 
                sp => sp.GetOrThrow<TController>(
                    $"Error in message processing for consumer group {groupId}");
        }

        public void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config)
        {
            _internalRegistry.ConsumerGroupConfigProviders[groupId] = config;
        }

        public void AddKafkaConsumer<TMessage>(string groupId, ConsumerConfig confluentConfig)
        {
            _internalRegistry.KafkaConsumerFactories[groupId] = sp =>
                new KafkaConsumer<TMessage>(
                    BuildConfluentConsumer<TMessage>(groupId, confluentConfig, sp),
                    _internalRegistry.ConsumerGroupConfigProviders[groupId],
                    new TimeoutRunner());
        }

        public void AddIntakeScope<TMessage>(string groupId)
        {
            _internalRegistry.IntakeScopeFactories[groupId] = sp =>
                new IntakeScope<TMessage>(
                    sp.GetRequiredService<IServiceScopeFactory>().CreateScope(),
                    scopedSp => (IKafkaIntakeStrategy<TMessage>)_internalRegistry.KafkaIntakeStrategyFactories[groupId](scopedSp),
                    scopedSp => (IKafkaIntakeThrottle)_internalRegistry.KafkaIntakeThrottleProviders[groupId](scopedSp),
                    scopedSp => (IKafkaController<TMessage>)_internalRegistry.KafkaControllerProviders[groupId](scopedSp));
        }

        public void AddDeadLetterMessageKey<TKey, TMessage>(string groupId, Func<TMessage, TKey> messageKeyProvider)
        {
            _internalRegistry.DeadLetterMessageKeyProviders[groupId] = messageKeyProvider;
        }

        public void AddJsonDeadLetterMessageSerializer<TMessage>(string groupId)
        {
            _internalRegistry
                .DeadLetterMessageSerializerProviders[groupId] = _ => new JsonSerializer<TMessage>();
        }

        public void AddCustomDeadLetterMessageSerializer<TSerializer>(string groupId) where TSerializer : class
        {
            if (_externalRegistry.All(s => s.ImplementationType != typeof(TSerializer)))
            {
                _externalRegistry.AddTransient<TSerializer>();
            }
            _internalRegistry.DeadLetterMessageSerializerProviders[groupId] =
                sp => sp.GetOrThrow<TSerializer>(
                    $"Error in custom dead-letter message serialization for consumer group {groupId}");
        }

        public void AddDeadLetterProducer<TKey, TMessage>(
            string groupId,
            ProduceConfig config,
            ProducerConfig confluentConfig)
        {
            _internalRegistry.DeadLetterProducerProviders[groupId] = new LazyFunc<IServiceProvider, object>(
                sp => new KafkaProducer<TKey, TMessage>(
                    BuildDeadLetterConfluentProducer<TKey, TMessage>(groupId, confluentConfig, sp),
                    (Func<TMessage, TKey>)_internalRegistry.DeadLetterMessageKeyProviders[groupId],
                    config));
        }

        public void AddKafkaWorker<TMessage>(string groupId)
        {
            _internalRegistry.KafkaWorkerFactories[groupId] = (sp, consumerId) =>
                new KafkaWorker<TMessage>(
                    groupId,
                    consumerId,
                    _internalRegistry.ConsumerGroupConfigProviders[groupId].ErrorHandling,
                    () => (IKafkaConsumer<TMessage>)_internalRegistry.KafkaConsumerFactories[groupId](sp),
                    () => (IntakeScope<TMessage>)_internalRegistry.IntakeScopeFactories[groupId](sp),
                    () => (IKafkaProducer<TMessage>)_internalRegistry.DeadLetterProducerProviders[groupId].Invoke(sp),
                    sp.GetRequiredService<ILogger<KafkaWorker<TMessage>>>());
        }

        private IConsumer<Ignore, TMessage> BuildConfluentConsumer<TMessage>(
            string groupId,
            ConsumerConfig config,
            IServiceProvider serviceProvider)
        {
            var builder = new ConsumerBuilder<Ignore, TMessage>(config);
            var deserializer = (IDeserializer<TMessage>)_internalRegistry
                .MessageDeserializerProviders[groupId](serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<IConsumer<Ignore, TMessage>>>();
            return builder
                .SetValueDeserializer(deserializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }

        private IProducer<TKey, TMessage> BuildDeadLetterConfluentProducer<TKey, TMessage>(
            string groupId,
            ProducerConfig config,
            IServiceProvider serviceProvider)
        {
            var builder = new ProducerBuilder<TKey, TMessage>(config);
            var serializer = (ISerializer<TMessage>)_internalRegistry
                .DeadLetterMessageSerializerProviders[groupId](serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<IProducer<TKey, TMessage>>>();
            return builder
                .SetValueSerializer(serializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }
    }
}
