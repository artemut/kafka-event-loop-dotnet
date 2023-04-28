namespace Kafka.EventLoop.Utils
{
    internal class TimeoutRunner : ITimeoutRunner
    {
        public async Task RunAsync(
            Action action,
            TimeSpan timeout,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            var actionTask = Task.Run(action, cancellationToken);
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var firstTask = await Task.WhenAny(actionTask, timeoutTask);

            cancellationToken.ThrowIfCancellationRequested();

            if (firstTask != actionTask)
            {
                throw new TimeoutException(errorMessage);
            }
        }
    }
}
