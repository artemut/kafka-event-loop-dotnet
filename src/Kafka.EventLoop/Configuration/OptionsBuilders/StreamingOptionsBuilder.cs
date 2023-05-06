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
        private bool _hasSerializerType;

        public StreamingOptionsBuilder(
            string groupId,
            StreamingConfig config,
            IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _config = config;
            _dependencyRegistrar = dependencyRegistrar;
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
            _dependencyRegistrar.AddCustomStreamingMessageSerializer<TSerializer>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IStreamingOptions Build()
        {
            if (!_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Outgoing stream message serializer is not specified for consumer group {_groupId}");
            }

            var confluentConfig = new ProducerConfig
            {
                BootstrapServers = _config.ConnectionString,
                AllowAutoCreateTopics = false,
                RequestTimeoutMs = 2000, // todo: make configurable
                SocketTimeoutMs = 2000, // todo: make configurable
                MessageTimeoutMs = 2000, // todo: make configurable
                EnableDeliveryReports = true,
                Acks = _config.AckLevel switch
                {
                    ProduceAckLevel.AllInSyncReplicas => Acks.All,
                    null or ProduceAckLevel.LeaderReplica => Acks.Leader,
                    _ => Acks.Leader
                }
            };
            _dependencyRegistrar.AddStreamingProducer<TOutMessage>(
                _groupId,
                _config,
                confluentConfig);

            return new StreamingOptions();
        }
    }
}
