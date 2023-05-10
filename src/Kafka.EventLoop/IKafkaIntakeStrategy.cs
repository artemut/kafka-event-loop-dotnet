namespace Kafka.EventLoop
{
    public interface IKafkaIntakeStrategy<TMessage> : IDisposable
    {
        CancellationToken Token { get; }
        void OnConsumeStarting();
        void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo);
    }
}
