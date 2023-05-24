using System.Diagnostics;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure
{
    internal class BlockingTimer : IDisposable
    {
        private readonly Func<Task> _action;
        private readonly TimeSpan _interval;
        private readonly CancellationTokenSource _cts;

        public BlockingTimer(Func<Task> action, TimeSpan interval)
        {
            _action = action;
            _interval = interval;
            _cts = new CancellationTokenSource();

            Task.Run(RunAsync);
        }

        private async Task RunAsync()
        {
            var stopwatch = new Stopwatch();
            while (!_cts.IsCancellationRequested)
            {
                stopwatch.Restart();

                await _action();

                var elapsed = stopwatch.Elapsed;
                if (elapsed < _interval)
                    await Task.Delay(_interval - elapsed);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
