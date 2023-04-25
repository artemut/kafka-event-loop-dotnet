using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder<TMessage> : IConsumerGroupOptionsBuilder<TMessage>
    {
        private readonly string _groupId;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private bool _hasDeserializerType;
        private bool _hasControllerType;

        public ConsumerGroupOptionsBuilder(string groupId, IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _dependencyRegistrar = dependencyRegistrar;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer()
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddJsonMessageDeserializer<TMessage>(_groupId);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasCustomMessageDeserializer<TDeserializer>()
            where TDeserializer : class, IDeserializer<TMessage>
        {
            if (_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Message deserializer is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomMessageDeserializer<TDeserializer>(_groupId);
            _hasDeserializerType = true;
            return this;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : class, IKafkaController<TMessage>
        {
            if (_hasControllerType)
            {
                throw new InvalidOperationException(
                    $"Controller is already configured for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddKafkaController<TController>(_groupId);
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
                    $"Missing controller configuration for consumer group {_groupId}");
            }
            if (!_hasDeserializerType)
            {
                throw new InvalidOperationException(
                    $"Missing message deserializer configuration for consumer group {_groupId}");
            }

            _dependencyRegistrar.AddConsumerConfig(_groupId, new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = "", // todo
                AutoOffsetReset = AutoOffsetReset.Earliest, // todo
                EnableAutoCommit = false // todo
            });
            _dependencyRegistrar.AddKafkaConsumer<TMessage>(_groupId);
            _dependencyRegistrar.AddIntakeScope<TMessage>(_groupId);
            _dependencyRegistrar.AddKafkaWorker<TMessage>(_groupId);

            return new ConsumerGroupOptions(_groupId);
        }
    }
}
