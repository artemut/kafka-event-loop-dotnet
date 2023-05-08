using Confluent.Kafka;
using Kafka.EventLoop.Configuration;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;
using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        private readonly IConsumer<Ignore, TMessage> _consumer;
        private readonly ConsumerGroupConfig _consumerGroupConfig;
        private readonly ITimeoutRunner _timeoutRunner;

        public KafkaConsumer(
            string consumerName,
            IConsumer<Ignore, TMessage> consumer,
            ConsumerGroupConfig consumerGroupConfig,
            ITimeoutRunner timeoutRunner)
        {
            Name = consumerName;
            _consumer = consumer;
            _consumerGroupConfig = consumerGroupConfig;
            _timeoutRunner = timeoutRunner;
        }

        public string Name { get; }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_consumerGroupConfig.SubscribeTimeoutMs ?? Defaults.SubscribeTimeoutMs);
            try
            {
                await _timeoutRunner.RunAsync(
                    () => _consumer.Subscribe(_consumerGroupConfig.TopicName),
                    timeout,
                    $"Wasn't able to subscribe to the topic {_consumerGroupConfig.TopicName} within configured timeout {timeout}",
                    cancellationToken);
            }
            catch (KafkaException ex)
            {
                throw new ConnectivityException(
                    $"Error while subscribing to the topic {_consumerGroupConfig.TopicName}: {ex.Error.Code}",
                    ex.Error.IsFatal, ex);
            }
        }

        public MessageInfo<TMessage>[] CollectMessages(
            IKafkaIntakeStrategy<TMessage> intakeStrategy,
            CancellationToken cancellationToken)
        {
            var messages = new List<MessageInfo<TMessage>>();
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(intakeStrategy.Token, cancellationToken))
            {
                try
                {
                    while (true)
                    {
                        var result = _consumer.Consume(linkedCts.Token);
                        var messageInfo = new MessageInfo<TMessage>(
                            result.Message.Value,
                            result.Message.Timestamp.UtcDateTime,
                            result.Topic,
                            result.Partition,
                            result.Offset);
                        messages.Add(messageInfo);

                        intakeStrategy.OnNewMessageConsumed(messageInfo);
                        if (intakeStrategy.Token.IsCancellationRequested)
                            break;
                    }
                }
                catch (ConsumeException ex)
                {
                    throw new ConnectivityException(
                        $"Error while consuming messages from kafka: {ex.Error.Code}",
                        ex.Error.IsFatal, ex);
                }
                catch (OperationCanceledException) when (intakeStrategy.Token.IsCancellationRequested)
                {
                }
            }

            return messages.ToArray();
        }

        public async Task<List<TopicPartition>> GetCurrentAssignmentAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_consumerGroupConfig.GetCurrentAssignmentTimeoutMs ??
                                               Defaults.GetCurrentAssignmentTimeoutMs);
            try
            {
                List<TopicPartition>? assignment = null;
                await _timeoutRunner.RunAsync(
                    () => assignment = _consumer.Assignment,
                    timeout,
                    $"Wasn't able to get current consumer assignment within configured timeout {timeout}",
                    cancellationToken);
                return assignment ?? new List<TopicPartition>();
            }
            catch (KafkaException ex)
            {
                throw new ConnectivityException(
                    $"Error while getting consumer assignment: {ex.Error.Code}",
                    ex.Error.IsFatal, ex);
            }
        }

        public async Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken)
        {
            var offsets = messages
                .GroupBy(x => new TopicPartition(x.Topic, x.Partition))
                .Select(tpGroup => new TopicPartitionOffset(
                    tpGroup.Key,
                    new Offset(tpGroup.Max(tpo => tpo.Offset) + 1)))
                .ToList();

            var timeout = TimeSpan.FromSeconds(_consumerGroupConfig.CommitTimeoutMs ?? Defaults.CommitTimeoutMs);
            try
            {
                await _timeoutRunner.RunAsync(
                    () => _consumer.Commit(offsets),
                    timeout,
                    $"Wasn't able to commit offsets within configured timeout {timeout}",
                    cancellationToken);
            }
            catch (KafkaException ex)
            {
                throw new ConnectivityException(
                    $"Error while committing offsets to kafka: {ex.Error.Code}",
                    ex.Error.IsFatal, ex);
            }
        }

        public async Task SeekAsync(
            IDictionary<int, long> partitionToLastAllowedOffset,
            CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_consumerGroupConfig.SeekTimeoutMs ?? Defaults.SeekTimeoutMs);
            try
            {
                foreach (var (partition, lastAllowedOffset) in partitionToLastAllowedOffset)
                {
                    var seekTo = new TopicPartitionOffset(
                        _consumerGroupConfig.TopicName,
                        partition,
                        lastAllowedOffset + 1);

                    await _timeoutRunner.RunAsync(
                        () => _consumer.Seek(seekTo),
                        timeout,
                        $"Wasn't able to seek to offset within configured timeout {timeout}",
                        cancellationToken);
                }
            }
            catch (KafkaException ex)
            {
                throw new ConnectivityException(
                    $"Error while seeking to offset on kafka: {ex.Error.Code}",
                    ex.Error.IsFatal, ex);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_consumerGroupConfig.CloseTimeoutMs ?? Defaults.CloseTimeoutMs);
            try
            {
                await _timeoutRunner.RunAsync(
                    () => _consumer.Close(),
                    timeout,
                    $"Wasn't able to close the client within configured timeout {timeout}",
                    cancellationToken);
            }
            catch (KafkaException ex)
            {
                throw new ConnectivityException(
                    $"Error while closing kafka consumer: {ex.Error.Code}",
                    ex.Error.IsFatal, ex);
            }
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}
