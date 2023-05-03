namespace Kafka.EventLoop.Consume.Filtration
{
    internal interface IKafkaIntakeFilter<TMessage>
    {
        FiltrationResult<TMessage> FilterMessages(MessageInfo<TMessage>[] messages);
    }
}
