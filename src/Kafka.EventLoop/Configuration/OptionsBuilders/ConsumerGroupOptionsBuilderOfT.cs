using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Configuration.Helpers;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder<TMessage> : IConsumerGroupOptionsBuilder<TMessage>
    {
        private readonly string _groupId;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly string _connectionString;
        private readonly ConsumerGroupConfig _consumerGroupConfig;
        private readonly ConsumerConfig _confluentConfig;
        private bool _hasDeserializerType;
        private bool _hasCustomIntakeThrottleType;
        private bool _hasCustomIntakeStrategyType;
        private bool _hasControllerType;
        private bool _hasCustomIntakeObserverType;
        private Type? _controllerType;
        private IDeadLetteringOptions? _deadLetteringOptions;
        private IStreamingOptions? _streamingOptions;

        public ConsumerGroupOptionsBuilder(
            string groupId,
            IDependencyRegistrar dependencyRegistrar,
            string connectionString,
            ConsumerGroupConfig consumerGroupConfig)
        {
            _groupId = groupId;
            _dependencyRegistrar = dependencyRegistrar;
            _connectionString = connectionString;
            _consumerGroupConfig = consumerGroupConfig;
            _confluentConfig = new ConsumerConfig();
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer()
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOptionsException(
                    $"Message deserializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddJsonMessageDeserializer<TMessage>(_groupId);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomMessageDeserializer<TDeserializer>()
            where TDeserializer : class, IDeserializer<TMessage?>
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOptionsException(
                    $"Message deserializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomMessageDeserializer<TDeserializer, TMessage>(_groupId);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeStrategy<TStrategy>()
            where TStrategy : class, IKafkaIntakeStrategy<TMessage>
        {
            if (_consumerGroupConfig.Intake?.Strategy?.IsDefault() == true)
            {
                throw new InvalidOptionsException(
                    $"Cannot use custom intake strategy because consumer group {_groupId} " +
                    $"is configured with default intake strategy: {_consumerGroupConfig.Intake.Strategy.Name}. " +
                    "Either remove default strategy from the settings or do not use custom intake strategy.");
            }
            if (_hasCustomIntakeStrategyType)
            {
                throw new InvalidOptionsException(
                    $"Custom intake strategy is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomIntakeStrategy<TStrategy, TMessage>(_groupId);
            _hasCustomIntakeStrategyType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomThrottle<TThrottle>()
            where TThrottle : class, IKafkaIntakeThrottle
        {
            if (_hasCustomIntakeThrottleType)
            {
                throw new InvalidOptionsException(
                    $"Custom intake throttle is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomIntakeThrottle<TThrottle>(_groupId);
            _hasCustomIntakeThrottleType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : class, IKafkaController<TMessage>
        {
            if (_hasControllerType)
            {
                throw new InvalidOptionsException(
                    $"Controller is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddKafkaController<TController, TMessage>(_groupId);
            _hasControllerType = true;
            _controllerType = typeof(TController);
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasDeadLettering<TMessageKey>(
            Func<IDeadLetteringOptionsBuilder<TMessageKey, TMessage>, IDeadLetteringOptions> optionsAction)
        {
            if (_deadLetteringOptions != null)
            {
                throw new InvalidOptionsException(
                    $"Dead lettering options are already provided for consumer group {_groupId}");
            }

            var deadLetteringConfig = _consumerGroupConfig.ErrorHandling?.DeadLettering;
            if (deadLetteringConfig == null)
            {
                throw new InvalidOptionsException(
                    $"Cannot use dead lettering options for consumer group {_groupId}. " +
                    "Settings do not contain corresponding critical error handling section");
            }

            var builder = new DeadLetteringOptionsBuilder<TMessageKey, TMessage>(
                _groupId,
                _connectionString,
                deadLetteringConfig,
                _dependencyRegistrar);
            _deadLetteringOptions = optionsAction(builder);

            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasStreaming<TOutMessage>(
            Func<IStreamingOptionsBuilder<TMessage, TOutMessage>, IStreamingOptions> optionsAction)
        {
            if (_streamingOptions != null)
            {
                throw new InvalidOptionsException(
                    $"Streaming options are already provided for consumer group {_groupId}");
            }

            var streamingConfig = _consumerGroupConfig.Streaming;
            if (streamingConfig == null)
            {
                throw new InvalidOptionsException(
                    $"Cannot use streaming options for consumer group {_groupId}. " +
                    "Settings do not contain corresponding streaming section");
            }

            var builder = new StreamingOptionsBuilder<TMessage, TOutMessage>(
                _groupId,
                _connectionString,
                streamingConfig,
                _dependencyRegistrar);
            _streamingOptions = optionsAction(builder);

            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasKafkaConfig(Action<ConsumerConfig> kafkaConfigAction)
        {
            kafkaConfigAction(_confluentConfig);

            if (!string.IsNullOrWhiteSpace(_confluentConfig.GroupId))
            {
                throw new InvalidOptionsException(
                    $"Please do not set {nameof(_confluentConfig.GroupId)} value when specifying kafka config. " +
                    $"Value is taken from the settings instead. Consumer group: {_groupId}");
            }
            if (!string.IsNullOrWhiteSpace(_confluentConfig.BootstrapServers))
            {
                throw new InvalidOptionsException(
                    $"Please do not set {nameof(_confluentConfig.BootstrapServers)} value when specifying kafka config. " +
                    $"Value is taken from the settings instead. Consumer group: {_groupId}");
            }
            if (_confluentConfig.EnableAutoCommit == true)
            {
                throw new InvalidOptionsException(
                    $"You specified {nameof(_confluentConfig.EnableAutoCommit)}=true which is not supported. " +
                    "Offsets are committed explicitly and after successful message processing only. " +
                    $"Consumer group: {_groupId}");
            }
            if (_confluentConfig.EnableAutoOffsetStore == true)
            {
                throw new InvalidOptionsException(
                    $"You specified {nameof(_confluentConfig.EnableAutoOffsetStore)}=true which is not supported. " +
                    "Offsets are committed explicitly and after successful message processing only. " +
                    $"Consumer group: {_groupId}");
            }
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeObserver<TObserver>()
            where TObserver : KafkaIntakeObserver<TMessage>
        {
            if (_hasCustomIntakeObserverType)
            {
                throw new InvalidOptionsException(
                    $"Custom intake observer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomIntakeObserver<TObserver, TMessage>(_groupId);
            _hasCustomIntakeObserverType = true;
            return this;
        }

        public IConsumerGroupOptions Build()
        {
            if (_consumerGroupConfig.Intake?.Strategy?.IsDefault() == true)
            {
                switch (_consumerGroupConfig.Intake.Strategy)
                {
                    case FixedSizeIntakeStrategyConfig config:
                        _dependencyRegistrar.AddFixedSizeIntakeStrategy<TMessage>(_groupId, config);
                        break;
                    case FixedIntervalIntakeStrategyConfig config:
                        _dependencyRegistrar.AddFixedIntervalIntakeStrategy<TMessage>(_groupId, config);
                        break;
                    case MaxSizeWithTimeoutIntakeStrategyConfig config:
                        _dependencyRegistrar.AddMaxSizeWithTimeoutIntakeStrategy<TMessage>(_groupId, config);
                        break;
                }
            }
            else if (!_hasCustomIntakeStrategyType)
            {
                throw new InvalidOptionsException(
                    "Either default intake strategy should be configured in the settings " +
                    $"for consumer group {_groupId} or custom intake strategy should be specified");
            }
            if (!_hasCustomIntakeThrottleType)
            {
                _dependencyRegistrar.AddDefaultIntakeThrottle(_groupId, _consumerGroupConfig.Intake);
            }
            if (!_hasControllerType)
            {
                throw new InvalidOptionsException(
                    $"Controller is not specified for consumer group {_groupId}");
            }
            if (!_hasDeserializerType)
            {
                throw new InvalidOptionsException(
                    $"Message deserializer is not specified for consumer group {_groupId}");
            }
            if (_consumerGroupConfig.ErrorHandling?.DeadLettering != null &&
                _deadLetteringOptions == null)
            {
                throw new InvalidOptionsException(
                    $"Missing dead lettering options for consumer group {_groupId}");
            }
            if (_consumerGroupConfig.Streaming != null && _streamingOptions == null)
            {
                throw new InvalidOptionsException(
                    $"Missing streaming options for consumer group {_groupId}");
            }
            if (_streamingOptions != null && !_controllerType.IsKafkaStreamingController())
            {
                throw new InvalidOptionsException(
                    $"Controller of type {_controllerType?.Name} must be a kafka streaming controller " +
                    $"when using streaming for consumer group {_groupId}");
            }
            
            _dependencyRegistrar.AddConsumerGroupConfig(_groupId, _consumerGroupConfig);

            _confluentConfig.GroupId = _groupId;
            _confluentConfig.BootstrapServers = _consumerGroupConfig.ConnectionString ?? _connectionString;
            _confluentConfig.EnableAutoCommit = false;
            _confluentConfig.EnableAutoOffsetStore = false;
            _dependencyRegistrar.AddKafkaConsumer<TMessage>(_groupId, _confluentConfig);
            
            _dependencyRegistrar.AddKafkaWorker<TMessage>(_groupId);

            return new ConsumerGroupOptions(_groupId);
        }
    }
}
