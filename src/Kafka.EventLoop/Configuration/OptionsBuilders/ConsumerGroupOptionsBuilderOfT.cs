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
        private readonly ConsumerGroupConfig _consumerGroupConfig;
        private bool _hasDeserializerType;
        private bool _hasCustomIntakeThrottleType;
        private bool _hasCustomIntakeStrategyType;
        private bool _hasCustomPartitionMessagesFilterType;
        private bool _hasControllerType;
        private IDeadLetteringOptions? _deadLetteringOptions;

        public ConsumerGroupOptionsBuilder(
            string groupId,
            IDependencyRegistrar dependencyRegistrar,
            ConsumerGroupConfig consumerGroupConfig)
        {
            _groupId = groupId;
            _dependencyRegistrar = dependencyRegistrar;
            _consumerGroupConfig = consumerGroupConfig;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer()
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOptionsException(
                    $"Message deserializer is already configured for consumer group {_groupId}");
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
                    $"Message deserializer is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomMessageDeserializer<TDeserializer>(_groupId);
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
                    $"Custom intake strategy is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomIntakeStrategy<TStrategy>(_groupId);
            _hasCustomIntakeStrategyType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomPartitionMessagesFilter<TFilter>()
            where TFilter : class, IKafkaPartitionMessagesFilter<TMessage>
        {
            if (_hasCustomPartitionMessagesFilterType)
            {
                throw new InvalidOptionsException(
                    $"Custom partition messages filter is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomPartitionMessagesFilter<TFilter>(_groupId);
            _hasCustomPartitionMessagesFilterType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomThrottle<TThrottle>()
            where TThrottle : class, IKafkaIntakeThrottle
        {
            if (_hasCustomIntakeThrottleType)
            {
                throw new InvalidOptionsException(
                    $"Custom intake throttle is already configured for consumer group {_groupId}");
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
                    $"Controller is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddKafkaController<TController>(_groupId);
            _hasControllerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasDeadLettering<TMessageKey>(
            Func<IDeadLetteringOptionsBuilder<TMessageKey, TMessage>, IDeadLetteringOptions> optionsAction)
        {
            if (_deadLetteringOptions != null)
            {
                throw new InvalidOptionsException(
                    $"Dead lettering is already configured for consumer group {_groupId}");
            }

            var deadLetteringConfig = _consumerGroupConfig.ErrorHandling?.Critical?.DeadLettering;
            if (deadLetteringConfig == null)
            {
                throw new InvalidOptionsException(
                    $"Cannot configure dead lettering for consumer group {_groupId}. " +
                    "Settings don't contain corresponding critical error handling section");
            }

            var builder = new DeadLetteringOptionsBuilder<TMessageKey, TMessage>(
                _groupId,
                deadLetteringConfig,
                _dependencyRegistrar);
            _deadLetteringOptions = optionsAction(builder);

            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeObserver<TObserver>()
            where TObserver : IKafkaIntakeObserver<TMessage>
        {
            // todo:
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
                    $"Custom intake strategy must be provided for consumer group {_groupId} " +
                    "or a default intake strategy should be configured in the settings");
            }
            if (!_hasCustomIntakeThrottleType)
            {
                _dependencyRegistrar.AddDefaultIntakeThrottle(_groupId, _consumerGroupConfig.Intake);
            }
            if (!_hasControllerType)
            {
                throw new InvalidOptionsException(
                    $"Missing controller configuration for consumer group {_groupId}");
            }
            if (!_hasDeserializerType)
            {
                throw new InvalidOptionsException(
                    $"Missing message deserializer configuration for consumer group {_groupId}");
            }
            if (_consumerGroupConfig.ErrorHandling?.Critical?.DeadLettering != null &&
                _deadLetteringOptions == null)
            {
                throw new InvalidOptionsException(
                    $"Missing dead lettering configuration for consumer group {_groupId}");
            }

            _dependencyRegistrar.AddConsumerGroupConfig(_groupId, _consumerGroupConfig);
            _dependencyRegistrar.AddKafkaConsumer<TMessage>(_groupId, new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = _consumerGroupConfig.ConnectionString,
                AutoOffsetReset = _consumerGroupConfig.AutoOffsetReset,
                EnableAutoCommit = false
            });
            _dependencyRegistrar.AddIntakeScope<TMessage>(_groupId);
            _dependencyRegistrar.AddKafkaWorker<TMessage>(_groupId);

            return new ConsumerGroupOptions(_groupId);
        }
    }
}
