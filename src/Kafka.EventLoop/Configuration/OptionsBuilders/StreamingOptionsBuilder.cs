using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class StreamingOptionsBuilder<TInMessage, TOutMessage>
        : IStreamingOptionsBuilder<TInMessage, TOutMessage>
    {
        private readonly string _groupId;
        private readonly StreamingConfig _config;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly ProducerConfig _confluentConfig;
        private bool _hasSerializerType;

        public StreamingOptionsBuilder(
            string groupId,
            StreamingConfig config,
            IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _config = config;
            _dependencyRegistrar = dependencyRegistrar;
            _confluentConfig = new ProducerConfig();
        }

        public IStreamingOptionsBuilder<TInMessage, TOutMessage> HasJsonOutMessageSerializer()
        {
            if (_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Outgoing stream message serializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddJsonStreamingMessageSerializer<TOutMessage>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IStreamingOptionsBuilder<TInMessage, TOutMessage> HasCustomOutMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TOutMessage>
        {
            if (_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Outgoing stream message serializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomStreamingMessageSerializer<TSerializer, TOutMessage>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IStreamingOptionsBuilder<TInMessage, TOutMessage> HasKafkaConfig(Action<ProducerConfig> kafkaConfigAction)
        {
            kafkaConfigAction(_confluentConfig);

            if (!string.IsNullOrWhiteSpace(_confluentConfig.BootstrapServers))
            {
                throw new InvalidOptionsException(
                    $"Please do not set {nameof(_confluentConfig.BootstrapServers)} value " +
                    "when specifying kafka config for streaming. " +
                    $"Value is taken from the settings instead. Consumer group: {_groupId}");
            }
            return this;
        }

        public IStreamingOptions Build()
        {
            if (!_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Outgoing stream message serializer is not specified for consumer group {_groupId}");
            }
            
            _confluentConfig.BootstrapServers = _config.ConnectionString;
            _confluentConfig.EnableDeliveryReports ??= true;
            _confluentConfig.Acks ??= Acks.Leader;
            _dependencyRegistrar.AddStreamingProducer<TOutMessage>(
                _groupId,
                _config,
                _confluentConfig);

            return new StreamingOptions();
        }
    }
}
