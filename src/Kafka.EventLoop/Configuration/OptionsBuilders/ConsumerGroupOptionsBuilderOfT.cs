using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder<TMessage> : IConsumerGroupOptionsBuilder<TMessage>
    {
        private readonly string _name;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private bool _hasDeserializerType;
        private bool _hasControllerType;

        public ConsumerGroupOptionsBuilder(string name, IDependencyRegistrar dependencyRegistrar)
        {
            _name = name;
            _dependencyRegistrar = dependencyRegistrar;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer()
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_name}");
            }
            _dependencyRegistrar.AddJsonMessageDeserializer<TMessage>(_name);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomMessageDeserializer<TDeserializer>()
            where TDeserializer : class, IDeserializer<TMessage>
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_name}");
            }
            _dependencyRegistrar.AddCustomMessageDeserializer<TDeserializer>(_name);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : class, IKafkaController<TMessage>
        {
            if (_hasControllerType)
            {
                throw new InvalidOperationException(
                    $"Controller is already configured for consumer group {_name}");
            }
            _dependencyRegistrar.AddKafkaController<TController>(_name);
            _hasControllerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeStrategy<TStrategy>()
            where TStrategy : IKafkaIntakeStrategy<TMessage>
        {
            // todo:
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
            if (!_hasControllerType)
            {
                throw new InvalidOperationException(
                    $"Missing controller configuration for consumer group {_name}");
            }
            if (!_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Missing message deserializer configuration for consumer group {_name}");
            }

            _dependencyRegistrar.AddConsumerConfig(_name, new ConsumerConfig
            {
                GroupId = _name,
                BootstrapServers = "", // todo
                AutoOffsetReset = AutoOffsetReset.Earliest, // todo
                EnableAutoCommit = false // todo
            });
            _dependencyRegistrar.AddKafkaConsumer<TMessage>(_name);
            _dependencyRegistrar.AddIntakeScope<TMessage>(_name);
            _dependencyRegistrar.AddKafkaWorker<TMessage>(_name);

            return new ConsumerGroupOptions(_name);
        }
    }
}
