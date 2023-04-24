using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder<TMessage> : IConsumerGroupOptionsBuilder<TMessage>
    {
        private readonly string _name;
        private readonly SerializationType _serializationType;
        private readonly bool _ignoreExtraElements;
        private Type? _controllerType;
        private Type? _intakeStrategyType;
        private Type? _intakeObserverType;

        public ConsumerGroupOptionsBuilder(
            string name,
            SerializationType serializationType,
            bool ignoreExtraElements)
        {
            _name = name;
            _serializationType = serializationType;
            _ignoreExtraElements = ignoreExtraElements;
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
            return new ConsumerGroupOptions(
                _name,
                typeof(TMessage),
                _serializationType,
                _ignoreExtraElements,
                _controllerType,
                _intakeStrategyType,
                _intakeObserverType);
        }
    }
}
