using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        private readonly IConsumer<Ignore, TMessage> _consumer;
        private readonly ConsumerGroupConfig _consumerGroupConfig;
        private readonly ILogger<KafkaConsumer<TMessage>> _logger;

        public KafkaConsumer(
            IConsumer<Ignore, TMessage> consumer,
            ConsumerGroupConfig consumerGroupConfig,
            ILogger<KafkaConsumer<TMessage>> logger)
        {
            _consumer = consumer;
            _consumerGroupConfig = consumerGroupConfig;
            _logger = logger;
        }

        public Task SubscribeAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_consumerGroupConfig.TopicName);
            _logger.LogInformation($"Subscribed to topic {_consumerGroupConfig.TopicName}");

            return Task.CompletedTask;
        }

        public MessageInfo<TMessage>[] CollectMessages(CancellationToken cancellationToken)
        {
            var messages = new List<MessageInfo<TMessage>>();

            var result = _consumer.Consume(cancellationToken);
            messages.Add(new MessageInfo<TMessage>(
                result.Message.Value,
                result.Message.Timestamp.UtcDateTime,
                result.Topic,
                result.Partition,
                result.Offset));
            
            return messages.ToArray();
        }

        public Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _consumer.Close();
            _logger.LogInformation($"Disconnected from the topic {_consumerGroupConfig.TopicName}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
