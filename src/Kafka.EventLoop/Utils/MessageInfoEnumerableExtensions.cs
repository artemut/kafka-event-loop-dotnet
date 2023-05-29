using Confluent.Kafka;

namespace Kafka.EventLoop.Utils
{
    internal static class MessageInfoEnumerableExtensions
    {
        public static TopicPartitionOffset[] ToTheNextOffsets(this IEnumerable<MessageInfo> messages)
        {
            return messages
                .GroupBy(x => new TopicPartition(x.Topic, x.Partition))
                .Select(tpGroup => new TopicPartitionOffset(
                    tpGroup.Key,
                    new Offset(tpGroup.Max(tpo => tpo.Offset) + 1)))
                .ToArray();
        }

        public static TopicPartitionOffset[] ToTheStartOffsets(this IEnumerable<MessageInfo> messages)
        {
            return messages
                .GroupBy(x => new TopicPartition(x.Topic, x.Partition))
                .Select(tpGroup => new TopicPartitionOffset(
                    tpGroup.Key,
                    new Offset(tpGroup.Min(tpo => tpo.Offset))))
                .ToArray();
        }
    }
}
