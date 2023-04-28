namespace Kafka.EventLoop
{
    public interface IKafkaIntakeThrottle
    {
        Task WaitAsync(ThrottleOptions options, CancellationToken cancellationToken);
    }
}
