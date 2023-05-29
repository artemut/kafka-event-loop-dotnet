namespace Kafka.EventLoop
{
    public interface IKafkaIntakeCancellation
    {
        void CancelAfter(TimeSpan millisecondsDelay);
        void CancelAfter(int millisecondsDelay);
        void Cancel();
        void StopIntakeForPartition(MessageInfo message, bool include);
    }
}
