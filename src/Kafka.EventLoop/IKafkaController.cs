namespace Kafka.EventLoop
{
    public interface IKafkaController<TMessage>
    {
        Task<MessageProcessingResult> ProcessAsync(MessageInfo<TMessage>[] messages, CancellationToken token);
    }
}
