using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.Consume.Throttling
{
    internal class DefaultKafkaIntakeThrottle : IKafkaIntakeThrottle
    {
        private readonly int? _maxSpeed;
        private readonly IStopwatch _stopwatch;
        private readonly Func<TimeSpan, CancellationToken, Task> _delayTaskFactory;

        public DefaultKafkaIntakeThrottle(
            int? maxSpeed,
            IStopwatch stopwatch,
            Func<TimeSpan, CancellationToken, Task> delayTaskFactory)
        {
            _maxSpeed = maxSpeed;
            _stopwatch = stopwatch;
            _delayTaskFactory = delayTaskFactory;

            if (_maxSpeed.HasValue)
                _stopwatch.Start();
        }

        public async Task WaitAsync(ThrottleOptions options, CancellationToken cancellationToken)
        {
            if (!_maxSpeed.HasValue)
                return;

            var currentDuration = _stopwatch.Stop();
            var minDuration = GetMinDurationOfIntake(options);

            if (currentDuration >= minDuration)
                return;

            var remainingTime = minDuration - currentDuration;
            Console.WriteLine($"Throttling (remaining = {remainingTime})");
            var delayTask = _delayTaskFactory(remainingTime, cancellationToken);
            await delayTask;
        }

        private TimeSpan GetMinDurationOfIntake(ThrottleOptions options)
        {
            if (!_maxSpeed.HasValue)
                return TimeSpan.Zero;
            if (options.AssignedPartitionCount <= 0 || options.ConsumedMessageCount <= 0)
                return TimeSpan.Zero;

            var minDurationForSinglePartition = options.ConsumedMessageCount / (double)_maxSpeed.Value;
            return TimeSpan.FromSeconds(minDurationForSinglePartition / options.AssignedPartitionCount);
        }
    }
}
