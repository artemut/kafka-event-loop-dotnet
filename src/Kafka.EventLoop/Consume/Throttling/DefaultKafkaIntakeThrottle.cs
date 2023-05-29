using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.Consume.Throttling
{
    internal class DefaultKafkaIntakeThrottle : IKafkaIntakeThrottle
    {
        private readonly int? _maxSpeed;
        private readonly Func<IStopwatch> _stopwatchFactory;
        private readonly Func<TimeSpan, CancellationToken, Task> _delayTaskFactory;

        public DefaultKafkaIntakeThrottle(
            int? maxSpeed,
            Func<IStopwatch> stopwatchFactory,
            Func<TimeSpan, CancellationToken, Task> delayTaskFactory)
        {
            _maxSpeed = maxSpeed;
            _stopwatchFactory = stopwatchFactory;
            _delayTaskFactory = delayTaskFactory;
        }

        public async Task ControlSpeedAsync(Func<Task<ThrottleOptions>> manageable, CancellationToken cancellationToken)
        {
            if (!_maxSpeed.HasValue)
            {
                await manageable();
                return;
            }

            var stopwatch = _stopwatchFactory();
            stopwatch.Start();

            var options = await manageable();

            var currentDuration = stopwatch.Stop();
            var minDuration = GetMinDurationOfIntake(options);

            if (currentDuration >= minDuration)
                return;

            var remainingTime = minDuration - currentDuration;
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
