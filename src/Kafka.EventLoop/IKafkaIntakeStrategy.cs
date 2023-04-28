namespace Kafka.EventLoop
{
    public interface IKafkaIntakeStrategy<TMessage> : IDisposable
    {
        CancellationToken Token { get; }
        void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo);
    }
}
