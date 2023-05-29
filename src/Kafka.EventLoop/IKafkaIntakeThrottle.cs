namespace Kafka.EventLoop
{
    public interface IKafkaIntakeThrottle
    {
        Task ControlSpeedAsync(Func<Task<ThrottleOptions>> manageable, CancellationToken cancellationToken);
    }
}
