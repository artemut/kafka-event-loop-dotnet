using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class DeadLetteringOptionsBuilder<TMessage>
        : IDeadLetteringOptionsBuilder<TMessage>
    {
        private readonly string _groupId;
        private readonly string _connectionString;
        private readonly DeadLetteringConfig _config;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly ProducerConfig _confluentConfig;
        private bool _hasSerializerType;

        public DeadLetteringOptionsBuilder(
            string groupId,
            string connectionString,
            DeadLetteringConfig config,
            IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _connectionString = connectionString;
            _config = config;
            _dependencyRegistrar = dependencyRegistrar;
            _confluentConfig = new ProducerConfig();
        }

        public IDeadLetteringOptionsBuilder<TMessage> HasJsonDeadLetterMessageSerializer()
        {
            if (_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message serializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddJsonDeadLetterMessageSerializer<TMessage>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IDeadLetteringOptionsBuilder<TMessage> HasCustomDeadLetterMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TMessage>
        {
            if (_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message serializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomDeadLetterMessageSerializer<TSerializer, TMessage>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IDeadLetteringOptionsBuilder<TMessage> HasKafkaConfig(Action<ProducerConfig> kafkaConfigAction)
        {
            kafkaConfigAction(_confluentConfig);

            if (!string.IsNullOrWhiteSpace(_confluentConfig.BootstrapServers))
            {
                throw new InvalidOptionsException(
                    $"Please do not set {nameof(_confluentConfig.BootstrapServers)} value " +
                    "when specifying kafka config for dead-lettering. " +
                    $"Value is taken from the settings instead. Consumer group: {_groupId}");
            }
            return this;
        }

        public IDeadLetteringOptions Build()
        {
            if (!_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message serializer is not specified for consumer group {_groupId}");
            }
            
            _confluentConfig.BootstrapServers = _config.ConnectionString ?? _connectionString;
            _confluentConfig.EnableDeliveryReports ??= true;
            _confluentConfig.Acks ??= Acks.Leader;
            _dependencyRegistrar.AddDeadLetterProducer<TMessage>(
                _groupId,
                _config,
                _confluentConfig);

            return new DeadLetteringOptions();
        }
    }
}
