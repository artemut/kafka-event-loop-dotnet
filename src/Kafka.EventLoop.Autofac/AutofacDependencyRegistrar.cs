using Autofac;
using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Consume.IntakeStrategies;
using Kafka.EventLoop.Consume.Throttling;
using Kafka.EventLoop.Conversion;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Produce;
using Kafka.EventLoop.Streaming;
using Kafka.EventLoop.Utils;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Autofac
{
    internal class AutofacDependencyRegistrar : IDependencyRegistrar
    {
        private readonly ContainerBuilder _builder;
        private readonly KafkaControllerActivator _kafkaControllerActivator = new();

        public AutofacDependencyRegistrar(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public void AddCustomGlobalObserver<TObserver>() where TObserver : KafkaGlobalObserver
        {
            _builder
                .RegisterType<TObserver>()
                .As<KafkaGlobalObserver>()
                .SingleInstance();
        }

        public void AddJsonMessageDeserializer<TMessage>(string groupId)
        {
            _builder
                .RegisterType<JsonDeserializer<TMessage>>()
                .Named<IDeserializer<TMessage?>>(groupId)
                .SingleInstance();
        }

        public void AddCustomMessageDeserializer<TDeserializer, TMessage>(string groupId)
            where TDeserializer : class, IDeserializer<TMessage?>
        {
            _builder
                .RegisterType<TDeserializer>()
                .Named<IDeserializer<TMessage?>>(groupId)
                .SingleInstance();
        }

        public void AddCustomIntakeObserver<TObserver, TMessage>(string groupId)
            where TObserver : KafkaIntakeObserver<TMessage>
        {
            _builder
                .RegisterType<TObserver>()
                .Named<KafkaIntakeObserver<TMessage>>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddFixedSizeIntakeStrategy<TMessage>(string groupId, FixedSizeIntakeStrategyConfig config)
        {
            _builder
                .Register(_ => new FixedSizeIntakeStrategy<TMessage>(config.Size))
                .Named<IKafkaIntakeStrategy<TMessage>>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddFixedIntervalIntakeStrategy<TMessage>(string groupId, FixedIntervalIntakeStrategyConfig config)
        {
            _builder
                .Register(_ => new FixedIntervalIntakeStrategy<TMessage>(config.IntervalInMs))
                .Named<IKafkaIntakeStrategy<TMessage>>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddMaxSizeWithTimeoutIntakeStrategy<TMessage>(string groupId, MaxSizeWithTimeoutIntakeStrategyConfig config)
        {
            _builder
                .Register(_ => new MaxSizeWithTimeoutIntakeStrategy<TMessage>(config.MaxSize, config.TimeoutInMs))
                .Named<IKafkaIntakeStrategy<TMessage>>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddCustomIntakeStrategy<TStrategy, TMessage>(string groupId) 
            where TStrategy : class, IKafkaIntakeStrategy<TMessage>
        {
            _builder
                .RegisterType<TStrategy>()
                .Named<IKafkaIntakeStrategy<TMessage>>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddDefaultIntakeThrottle(string groupId, IntakeConfig? intakeConfig)
        {
            _builder
                .RegisterInstance(new DefaultKafkaIntakeThrottle(
                    intakeConfig?.MaxSpeed,
                    () => new StopwatchAdapter(),
                    Task.Delay))
                .Named<IKafkaIntakeThrottle>(groupId)
                .SingleInstance();
        }

        public void AddCustomIntakeThrottle<TThrottle>(string groupId)
            where TThrottle : class, IKafkaIntakeThrottle
        {
            _builder
                .RegisterType<TThrottle>()
                .Named<IKafkaIntakeThrottle>(groupId)
                .InstancePerLifetimeScope();
        }

        public void AddKafkaController<TController, TMessage>(string groupId)
            where TController : class, IKafkaController<TMessage>
        {
            _builder
                .RegisterType<TController>()
                .Named<IKafkaController<TMessage>>(groupId)
                .OnActivating(e => _kafkaControllerActivator.Activate(groupId, e.Context, e.Instance))
                .InstancePerLifetimeScope();
        }

        public void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config)
        {
            _builder
                .RegisterInstance(config)
                .Named<ConsumerGroupConfig>(groupId)
                .SingleInstance();
        }

        public void AddKafkaConsumer<TMessage>(string groupId, ConsumerConfig confluentConfig)
        {
            _builder
                .RegisterType<KafkaConsumer<TMessage>>()
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(IConsumer<Ignore, TMessage>),
                    (_, ctx) => BuildConfluentConsumer<TMessage>(groupId, confluentConfig, ctx))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(ConsumerGroupConfig),
                    (_, ctx) => ctx.ResolveNamed<ConsumerGroupConfig>(groupId))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(ITimeoutRunner),
                    (_, _) => new TimeoutRunner())
                .Named<IKafkaConsumer<TMessage>>(groupId);
        }

        public void AddDeadLetterMessageKey<TKey, TMessage>(string groupId, Func<TMessage, TKey> messageKeyProvider)
        {
            _builder
                .RegisterInstance(messageKeyProvider)
                .Named<Func<TMessage, TKey>>(DeadLetteringName(groupId))
                .SingleInstance();
        }

        public void AddJsonDeadLetterMessageSerializer<TMessage>(string groupId)
        {
            _builder
                .RegisterInstance(new JsonSerializer<TMessage>())
                .Named<ISerializer<TMessage>>(DeadLetteringName(groupId))
                .SingleInstance();
        }

        public void AddCustomDeadLetterMessageSerializer<TSerializer, TMessage>(string groupId)
            where TSerializer : class, ISerializer<TMessage>
        {
            _builder
                .RegisterType<TSerializer>()
                .Named<ISerializer<TMessage>>(DeadLetteringName(groupId));
        }

        public void AddDeadLetterProducer<TKey, TMessage>(
            string groupId,
            ProduceConfig config,
            ProducerConfig confluentConfig)
        {
            _builder
                .RegisterType<KafkaProducer<TKey, TMessage>>()
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(IProducer<TKey, TMessage>),
                    (_, ctx) => BuildDeadLetterConfluentProducer<TKey, TMessage>(groupId, confluentConfig, ctx))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(Func<TMessage, TKey>),
                    (_, ctx) => ctx.ResolveNamed<Func<TMessage, TKey>>(DeadLetteringName(groupId)))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(ProduceConfig),
                    (_, _) => config)
                .Named<IKafkaProducer<TMessage>>(DeadLetteringName(groupId));
        }

        public void AddJsonStreamingMessageSerializer<TOutMessage>(string groupId)
        {
            _builder
                .RegisterInstance(new JsonSerializer<TOutMessage>())
                .Named<ISerializer<TOutMessage>>(StreamingName(groupId))
                .SingleInstance();
        }

        public void AddCustomStreamingMessageSerializer<TSerializer, TOutMessage>(string groupId)
            where TSerializer : class, ISerializer<TOutMessage>
        {
            _builder
                .RegisterType<TSerializer>()
                .Named<ISerializer<TOutMessage>>(StreamingName(groupId));
        }

        public void AddStreamingProducer<TOutMessage>(
            string groupId,
            ProduceConfig config,
            ProducerConfig confluentConfig)
        {
            // todo: currently Ignore key is used as it is possible for one-to-one streaming, consider extending

            _builder
                .RegisterType<KafkaProducer<Ignore, TOutMessage>>()
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(IProducer<Ignore, TOutMessage>),
                    (_, ctx) => BuildStreamingConfluentProducer<TOutMessage>(groupId, confluentConfig, ctx))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(Func<TOutMessage, Ignore>),
                    (_, _) => new Func<TOutMessage, Ignore>(_ => null!))
                .WithParameter(
                    (p, _) => p.ParameterType == typeof(ProduceConfig),
                    (_, _) => config)
                .Named<IKafkaProducer<TOutMessage>>(StreamingName(groupId));

            // inject producer into the corresponding kafka streaming controller
            _kafkaControllerActivator.Register(groupId, (ctx, controller) =>
            {
                if (controller is IStreamingInjection<TOutMessage> injection)
                {
                    var producer = ctx.ResolveNamed<IKafkaProducer<TOutMessage>>(StreamingName(groupId));
                    injection.InjectStreamingProducer(producer);
                }
            });
        }

        public void AddKafkaWorker<TMessage>(string groupId)
        {
            _builder
                .Register<IKafkaConsumer<TMessage>, IKafkaIntake>((ctx, consumer) =>
                {
                    var lifetimeScope = ctx.Resolve<ILifetimeScope>().BeginLifetimeScope();
                    var intake = new KafkaIntake<TMessage>(
                        consumer,
                        () => lifetimeScope.ResolveOptionalNamed<KafkaIntakeObserver<TMessage>>(groupId),
                        () => lifetimeScope.ResolveNamed<IKafkaIntakeStrategy<TMessage>>(groupId),
                        () => lifetimeScope.ResolveNamed<IKafkaIntakeThrottle>(groupId),
                        () => lifetimeScope.ResolveNamed<IKafkaController<TMessage>>(groupId),
                        () => lifetimeScope.ResolveNamed<IKafkaProducer<TMessage>>(DeadLetteringName(groupId)),
                        lifetimeScope.ResolveNamed<ConsumerGroupConfig>(groupId).ErrorHandling,
                        lifetimeScope.Resolve<ILogger<KafkaIntake<TMessage>>>());
                    return new KafkaIntakeLifetimeScopeDecorator(lifetimeScope, intake);
                })
                .Named<IKafkaIntake>(groupId);

            _builder
                .Register<ConsumerId, IKafkaWorker>((ctx, consumerId) =>
                {
                    var newCtx = ctx.Resolve<IComponentContext>();
                    return new KafkaWorker<TMessage>(
                        consumerId,
                        () => newCtx.ResolveNamed<Func<ConsumerId, IKafkaConsumer<TMessage>>>(groupId)(consumerId),
                        consumer => newCtx.ResolveNamed<Func<IKafkaConsumer<TMessage>, IKafkaIntake>>(groupId)(consumer),
                        ctx.ResolveOptional<KafkaGlobalObserver>(),
                        ctx.Resolve<ILogger<KafkaWorker<TMessage>>>());
                })
                .Named<IKafkaWorker>(groupId);

            _builder
                .Register<Func<ConsumerId, IKafkaWorker>>(ctx =>
                {
                    var newCtx = ctx.Resolve<IComponentContext>();
                    return consumerId => newCtx.ResolveNamed<Func<ConsumerId, IKafkaWorker>>(consumerId.GroupId)(consumerId);
                });
        }

        public void AddKafkaService(KafkaConfig kafkaConfig)
        {
            _builder.RegisterInstance(kafkaConfig).SingleInstance();
            _builder.RegisterType<KafkaBackgroundService>().SingleInstance();
        }

        private IConsumer<Ignore, TMessage> BuildConfluentConsumer<TMessage>(
            string groupId,
            ConsumerConfig config,
            IComponentContext context)
        {
            var builder = new ConsumerBuilder<Ignore, TMessage>(config);
            var deserializer = context.ResolveNamed<IDeserializer<TMessage>>(groupId);
            var logger = context.Resolve<ILogger<IConsumer<Ignore, TMessage>>>();
            return builder
                .SetValueDeserializer(deserializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }

        private IProducer<TKey, TMessage> BuildDeadLetterConfluentProducer<TKey, TMessage>(
            string groupId,
            ProducerConfig config,
            IComponentContext context)
        {
            var builder = new ProducerBuilder<TKey, TMessage>(config);
            var serializer = context.ResolveNamed<ISerializer<TMessage>>(DeadLetteringName(groupId));
            var logger = context.Resolve<ILogger<IProducer<TKey, TMessage>>>();
            return builder
                .SetValueSerializer(serializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }

        private IProducer<Ignore, TOutMessage> BuildStreamingConfluentProducer<TOutMessage>(
            string groupId,
            ProducerConfig config,
            IComponentContext context)
        {
            var builder = new ProducerBuilder<Ignore, TOutMessage>(config);
            var serializer = context.ResolveNamed<ISerializer<TOutMessage>>(StreamingName(groupId));
            var logger = context.Resolve<ILogger<IProducer<Ignore, TOutMessage>>>();
            return builder
                .SetKeySerializer(new IgnoreSerializer())
                .SetValueSerializer(serializer)
                .SetLogHandler((_, msg) => logger.Log(msg.Level.ToLogLevel(), "{Message}", msg.Message))
                .Build();
        }

        private static string DeadLetteringName(string groupId) => $"{groupId}:dead-lettering";
        private static string StreamingName(string groupId) => $"{groupId}:streaming";
    }
}
