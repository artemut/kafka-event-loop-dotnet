using System.Diagnostics;

namespace Kafka.EventLoop.Utils
{
    internal class StopwatchAdapter : IStopwatch
    {
        private Stopwatch? _stopwatch;

        public void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public TimeSpan Stop()
        {
            if (_stopwatch == null)
                throw new InvalidOperationException("Stopwatch wasn't started");
            _stopwatch.Stop();
            return _stopwatch.Elapsed;
        }
    }
}
