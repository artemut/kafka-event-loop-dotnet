namespace Kafka.EventLoop
{
    public interface IKafkaPartitionMessagesFilter<TMessage>
    {
        MessageInfo<TMessage> GetLastAllowedMessageForPartition(
            IEnumerable<MessageInfo<TMessage>> partitionMessages);
    }
}
