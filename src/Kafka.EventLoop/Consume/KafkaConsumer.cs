using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        private readonly IConsumer<Ignore, TMessage> _consumer;
        private readonly ConsumerGroupConfig _consumerGroupConfig;
        private readonly ITimeoutRunner _timeoutRunner;

        public KafkaConsumer(
            IConsumer<Ignore, TMessage> consumer,
            ConsumerGroupConfig consumerGroupConfig,
            ITimeoutRunner timeoutRunner)
        {
            _consumer = consumer;
            _consumerGroupConfig = consumerGroupConfig;
            _timeoutRunner = timeoutRunner;
        }

        public Task SubscribeAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(2); // todo: make configurable
            return _timeoutRunner.RunAsync(
                () => _consumer.Subscribe(_consumerGroupConfig.TopicName),
                timeout,
                $"Wasn't able to subscribe to the topic {_consumerGroupConfig.TopicName} within configured timeout {timeout}",
                cancellationToken);
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
            var offsets = messages
                .GroupBy(x => new TopicPartition(x.Topic, x.Partition))
                .Select(tpGroup => new TopicPartitionOffset(
                    tpGroup.Key,
                    new Offset(tpGroup.Max(tpo => tpo.Offset) + 1)))
                .ToList();

            var timeout = TimeSpan.FromSeconds(2); // todo: make configurable
            return _timeoutRunner.RunAsync(
                () => _consumer.Commit(offsets),
                timeout,
                $"Wasn't able to commit offsets after configured timeout {timeout}",
                cancellationToken);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(2); // todo: make configurable
            return _timeoutRunner.RunAsync(
                () => _consumer.Close(),
                timeout,
                $"Wasn't able to close the client after configured timeout {timeout}",
                cancellationToken);
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
