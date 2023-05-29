namespace Kafka.EventLoop.Core
{
    internal interface IKafkaWorker
    {
        Task RunAsync(CancellationToken stoppingToken);
    }
}
