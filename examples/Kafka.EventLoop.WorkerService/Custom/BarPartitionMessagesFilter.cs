using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class BarPartitionMessagesFilter : IKafkaPartitionMessagesFilter<BarMessage>
    {
        public MessageInfo<BarMessage> GetLastAllowedMessageForPartition(
            IEnumerable<MessageInfo<BarMessage>> partitionMessages)
        {
            // allow only the very first message in each partition
            return partitionMessages.First();
        }
    }
}
