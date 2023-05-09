using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooPartitionMessagesFilter : IKafkaPartitionMessagesFilter<FooMessage>
    {
        public MessageInfo<FooMessage> GetLastAllowedMessageForPartition(
            IEnumerable<MessageInfo<FooMessage>> partitionMessages)
        {
            // allow only the very first message in each partition
            return partitionMessages.First();
        }
    }
}
