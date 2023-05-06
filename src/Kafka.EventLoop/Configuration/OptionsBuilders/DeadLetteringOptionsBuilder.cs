using System.Linq.Expressions;
using Kafka.EventLoop.Configuration.Options;
using Confluent.Kafka;
using Kafka.EventLoop.DependencyInjection;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class DeadLetteringOptionsBuilder<TMessageKey, TMessage>
        : IDeadLetteringOptionsBuilder<TMessageKey, TMessage>
    {
        private readonly string _groupId;
        private readonly DeadLetteringConfig _config;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private bool _hasMessageKeyType;
        private bool _hasSerializerType;

        public DeadLetteringOptionsBuilder(
            string groupId,
            DeadLetteringConfig config,
            IDependencyRegistrar dependencyRegistrar)
        {
            _groupId = groupId;
            _config = config;
            _dependencyRegistrar = dependencyRegistrar;
        }

        public IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasDeadLetterMessageKey(
            Expression<Func<TMessage, TMessageKey>> messageKey)
        {
            if (_hasMessageKeyType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message key is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddDeadLetterMessageKey(_groupId, messageKey.Compile());
            _hasMessageKeyType = true;
            return this;
        }

        public IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasJsonDeadLetterMessageSerializer()
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

        public IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasCustomDeadLetterMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TMessage>
        {
            if (_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message serializer is already specified for consumer group {_groupId}");
            }
            _dependencyRegistrar.AddCustomDeadLetterMessageSerializer<TSerializer>(_groupId);
            _hasSerializerType = true;
            return this;
        }

        public IDeadLetteringOptions Build()
        {
            if (!_hasMessageKeyType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message key is not specified for consumer group {_groupId}");
            }
            if (!_hasSerializerType)
            {
                throw new InvalidOptionsException(
                    $"Dead letter message serializer is not specified for consumer group {_groupId}");
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
            _dependencyRegistrar.AddDeadLetterProducer<TMessageKey, TMessage>(
                _groupId,
                _config,
                confluentConfig);

            return new DeadLetteringOptions();
        }
    }
}
