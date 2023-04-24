namespace Kafka.EventLoop
{
    public interface IKafkaController<in TMessage>
    {
        Task ProcessAsync(TMessage[] messages, CancellationToken token);
    }
}
