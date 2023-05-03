namespace Kafka.EventLoop.Consume.Filtration
{
    internal record FiltrationResult<TMessage>(
        MessageInfo<TMessage>[] Messages,
        IDictionary<int, long>? PartitionToLastAllowedOffset = null);
}
