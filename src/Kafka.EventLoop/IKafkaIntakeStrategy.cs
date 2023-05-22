namespace Kafka.EventLoop
{
    public interface IKafkaIntakeStrategy<TMessage>
    {
        void OnConsumeStarting(IKafkaIntakeCancellation cancellation);
        void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo);
    }
}
