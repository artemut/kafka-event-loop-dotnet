namespace Kafka.EventLoop.Utils
{
    internal interface ITimeoutRunner
    {
        Task RunAsync(
            Action action,
            TimeSpan timeout,
            string errorMessage,
            CancellationToken cancellationToken);
    }
}
