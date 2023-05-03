namespace Kafka.EventLoop
{
    public interface IKafkaController<TMessage>
    {
        Task ProcessAsync(MessageInfo<TMessage>[] messages, CancellationToken token);
    }
}
