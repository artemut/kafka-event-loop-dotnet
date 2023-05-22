using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Consume.Throttling;
using Kafka.EventLoop.Conversion;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.Produce;
using Kafka.EventLoop.Streaming;
using Kafka.EventLoop.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.DependencyInjection.Default
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

        public void AddCustomGlobalObserver<TObserver>() where TObserver : KafkaGlobalObserver
        {
            if (!_externalRegistry.IsRegistered<TObserver>())
            {
                _externalRegistry.AddSingleton<TObserver>();
            }
            _internalRegistry.KafkaGlobalObserver = sp => sp.GetOrThrow<TObserver>("Error in custom global observer");
        }

        public void AddJsonMessageDeserializer<TMessage>(string groupId)
        {
            _internalRegistry
                .MessageDeserializerProviders[groupId] = _ => new JsonDeserializer<TMessage>();
        }

        public void AddCustomMessageDeserializer<TDeserializer, TMessage>(string groupId)
            where TDeserializer : class, IDeserializer<TMessage?>
        {
            if (!_externalRegistry.IsRegistered<TDeserializer>())
            {
                _externalRegistry.AddTransient<TDeserializer>();
            }
            _internalRegistry.MessageDeserializerProviders[groupId] =
                sp => sp.GetOrThrow<TDeserializer>(
                    $"Error in custom message deserialization for consumer group {groupId}");
        }

        public void AddCustomIntakeObserver<TObserver, TMessage>(string groupId)
            where TObserver : KafkaIntakeObserver<TMessage>
        {
            if (!_externalRegistry.IsRegistered<TObserver>())
            {
                _externalRegistry.AddScoped<TObserver>();
            }
            _internalRegistry.KafkaIntakeObserverFactories[groupId] =
                sp => sp.GetOrThrow<TObserver>(
                    $"Error in custom intake observer for consumer group {groupId}");
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

        public void AddCustomIntakeStrategy<TStrategy, TMessage>(string groupId)
            where TStrategy : class, IKafkaIntakeStrategy<TMessage>
        {
            if (!_externalRegistry.IsRegistered<TStrategy>())
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

        public void AddCustomIntakeThrottle<TThrottle>(string groupId)
            where TThrottle : class, IKafkaIntakeThrottle
        {
            if (!_externalRegistry.IsRegistered<TThrottle>())
            {
                _externalRegistry.AddScoped<TThrottle>();
            }
            _internalRegistry.KafkaIntakeThrottleProviders[groupId] =
                sp => sp.GetOrThrow<TThrottle>(
                    $"Error in custom intake throttle for consumer group {groupId}");
        }

        public void AddKafkaController<TController, TMessage>(string groupId)
            where TController : class, IKafkaController<TMessage>
        {
            if (!_externalRegistry.IsRegistered<TController>())
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
            _internalRegistry.KafkaConsumerFactories[groupId] = (sp, consumerId) =>
                new KafkaConsumer<TMessage>(
                    consumerId,
                    BuildConfluentConsumer<TMessage>(groupId, confluentConfig, sp),
                    _internalRegistry.ConsumerGroupConfigProviders[groupId],
                    new TimeoutRunner());
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

        public void AddCustomDeadLetterMessageSerializer<TSerializer, TMessage>(string groupId)
            where TSerializer : class, ISerializer<TMessage>
        {
            if (!_externalRegistry.IsRegistered<TSerializer>())
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

        public void AddJsonStreamingMessageSerializer<TOutMessage>(string groupId)
        {
            _internalRegistry
                .StreamingMessageSerializerProviders[groupId] = _ => new JsonSerializer<TOutMessage>();
        }

        public void AddCustomStreamingMessageSerializer<TSerializer, TOutMessage>(string groupId)
            where TSerializer : class, ISerializer<TOutMessage>
        {
            if (!_externalRegistry.IsRegistered<TSerializer>())
            {
                _externalRegistry.AddTransient<TSerializer>();
            }
            _internalRegistry.StreamingMessageSerializerProviders[groupId] =
                sp => sp.GetOrThrow<TSerializer>(
                    $"Error in custom streaming message serialization for consumer group {groupId}");
        }

        public void AddStreamingProducer<TOutMessage>(
            string groupId,
            ProduceConfig config,
            ProducerConfig confluentConfig)
        {
            // todo: currently Ignore key is used as it is possible for one-to-one streaming, consider extending

            _internalRegistry.StreamingProducerProviders[groupId] = new LazyFunc<IServiceProvider, object>(
                sp => new KafkaProducer<Ignore, TOutMessage>(
                    BuildStreamingConfluentProducer<TOutMessage>(groupId, confluentConfig, sp),
                    _ => null!,
                    config));

            // inject producer into the corresponding kafka streaming controller
            var originalProvider = _internalRegistry.KafkaControllerProviders[groupId];
            _internalRegistry.KafkaControllerProviders[groupId] = sp =>
            {
                var controller = originalProvider(sp);
                if (controller is IStreamingInjection<TOutMessage> injection)
                {
                    var producer = (IKafkaProducer<TOutMessage>)_internalRegistry.StreamingProducerProviders[groupId].Invoke(sp);
                    injection.InjectStreamingProducer(producer);
                }
                return controller;
            };
        }

        public void AddKafkaWorker<TMessage>(string groupId)
        {
            _internalRegistry.KafkaIntakeFactories[groupId] = (sp, consumer) =>
            {
                var serviceScope = sp.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var scopedSp = serviceScope.ServiceProvider;
                var intake = new KafkaIntake<TMessage>(
                    (IKafkaConsumer<TMessage>)consumer,
                    () =>
                    {
                        var factories = _internalRegistry.KafkaIntakeObserverFactories;
                        return (KafkaIntakeObserver<TMessage>?)(factories.ContainsKey(groupId)
                            ? factories[groupId](scopedSp)
                            : null);
                    },
                    () => (IKafkaIntakeStrategy<TMessage>)_internalRegistry.KafkaIntakeStrategyFactories[groupId](scopedSp),
                    () => (IKafkaIntakeThrottle)_internalRegistry.KafkaIntakeThrottleProviders[groupId](scopedSp),
                    () => (IKafkaController<TMessage>)_internalRegistry.KafkaControllerProviders[groupId](scopedSp),
                    () => (IKafkaProducer<TMessage>)_internalRegistry.DeadLetterProducerProviders[groupId].Invoke(sp),
                    _internalRegistry.ConsumerGroupConfigProviders[groupId].ErrorHandling,
                    scopedSp.GetRequiredService<ILogger<KafkaIntake<TMessage>>>());
                return new KafkaIntakeServiceScopeDecorator(serviceScope, intake);
            };

            _internalRegistry.KafkaWorkerFactories[groupId] = (sp, consumerId) =>
                new KafkaWorker<TMessage>(
                    consumerId,
                    _internalRegistry.ConsumerGroupConfigProviders[groupId].ErrorHandling,
                    () => (IKafkaConsumer<TMessage>)_internalRegistry.KafkaConsumerFactories[groupId](sp, consumerId),
                    consumer => (IKafkaIntake)_internalRegistry.KafkaIntakeFactories[groupId](sp, consumer),
                    _internalRegistry.KafkaGlobalObserver?.Invoke(sp),
                    sp.GetRequiredService<ILogger<KafkaWorker<TMessage>>>());
        }

        public void AddKafkaService(KafkaConfig kafkaConfig)
        {
            _externalRegistry.AddHostedService(sp => new KafkaBackgroundService(
                kafkaConfig,
                consumerId => _internalRegistry.KafkaWorkerFactories[consumerId.GroupId](sp, consumerId),
                sp.GetRequiredService<ILogger<KafkaBackgroundService>>()));
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

        private IProducer<Ignore, TOutMessage> BuildStreamingConfluentProducer<TOutMessage>(
            string groupId,
            ProducerConfig config,
            IServiceProvider serviceProvider)
        {
            var builder = new ProducerBuilder<Ignore, TOutMessage>(config);
            var serializer = (ISerializer<TOutMessage>)_internalRegistry
                .StreamingMessageSerializerProviders[groupId](serviceProvider);
            var logger = serviceProvider.GetRequiredService<ILogger<IProducer<Ignore, TOutMessage>>>();
            return builder
                .SetKeySerializer(new IgnoreSerializer())
                .SetValueSerializer(serializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }
    }
}
