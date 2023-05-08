namespace Kafka.EventLoop.Core
{
    internal interface IKafkaIntake : IDisposable
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
