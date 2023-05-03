using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Consume.Filtration
{
    internal class KafkaIntakeFilter<TMessage> : IKafkaIntakeFilter<TMessage>
    {
        private readonly IKafkaPartitionMessagesFilter<TMessage>? _partitionMessagesFilter;
        private readonly ILogger<KafkaIntakeFilter<TMessage>> _logger;

        public KafkaIntakeFilter(
            IKafkaPartitionMessagesFilter<TMessage>? partitionMessagesFilter,
            ILogger<KafkaIntakeFilter<TMessage>> logger)
        {
            _partitionMessagesFilter = partitionMessagesFilter;
            _logger = logger;
        }

        public FiltrationResult<TMessage> FilterMessages(MessageInfo<TMessage>[] messages)
        {
            if (_partitionMessagesFilter == null)
                return new FiltrationResult<TMessage>(messages);

            var partitionToLastAllowedOffset = new Dictionary<int, long>();
            foreach (var partitionToItsMessages in messages.GroupBy(m => m.Partition))
            {
                var lastMessage = _partitionMessagesFilter
                    .GetLastAllowedMessageForPartition(partitionToItsMessages);
                partitionToLastAllowedOffset.Add(partitionToItsMessages.Key, lastMessage.Offset);
            }

            var originalCount = messages.Length;
            messages = messages
                .Where(m => m.Offset <= partitionToLastAllowedOffset[m.Partition])
                .ToArray();
            
            var filteredCount = originalCount - messages.Length;
            _logger.LogDebug(filteredCount > 0
                ? $"{filteredCount} messages were filtered out of {originalCount} total"
                : $"No messages were filtered out of {originalCount} total");

            return new FiltrationResult<TMessage>(messages, partitionToLastAllowedOffset);
        }
    }
}
