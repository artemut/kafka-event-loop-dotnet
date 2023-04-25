using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.Conversion;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder<TMessage> : IConsumerGroupOptionsBuilder<TMessage>
    {
        private readonly string _name;
        private Type? _controllerType;
        private Type? _intakeStrategyType;
        private Type? _deserializerType;
        private Type? _intakeObserverType;

        public ConsumerGroupOptionsBuilder(string name)
        {
            _name = name;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer()
        {
            if (_deserializerType != null)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_name}");
            }
            _deserializerType = typeof(JsonDeserializer<TMessage>);
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomMessageDeserializer<TDeserializer>()
            where TDeserializer : IDeserializer<TMessage>
        {
            if (_deserializerType != null)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_name}");
            }
            _deserializerType = typeof(TDeserializer);
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : IKafkaController<TMessage>
        {
            if (_controllerType != null)
            {
                throw new InvalidOperationException(
                    $"Controller is already configured for consumer group {_name}");
            }
            _controllerType = typeof(TController);
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeStrategy<TStrategy>()
            where TStrategy : IKafkaIntakeStrategy<TMessage>
        {
            if (_intakeStrategyType != null)
            {
                throw new InvalidOperationException(
                    $"Custom intake strategy is already configured for consumer group {_name}");
            }
            _intakeStrategyType = typeof(TStrategy);
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeObserver<TObserver>()
            where TObserver : IKafkaIntakeObserver<TMessage>
        {
            if (_intakeObserverType != null)
            {
                throw new InvalidOperationException(
                    $"Custom intake observer is already configured for consumer group {_name}");
            }
            _intakeObserverType = typeof(TObserver);
            return this;
        }

        public IConsumerGroupOptions Build()
        {
            if (_controllerType == null)
            {
                throw new InvalidOperationException(
                    $"Missing controller configuration for consumer group {_name}");
            }
            if (_deserializerType == null)
            {
                throw new InvalidOperationException(
                    $"Missing message deserializer configuration for consumer group {_name}");
            }

            return new ConsumerGroupOptions(
                _name,
                typeof(TMessage),
                _deserializerType,
                _controllerType,
                _intakeStrategyType,
                _intakeObserverType);
        }
    }
}
