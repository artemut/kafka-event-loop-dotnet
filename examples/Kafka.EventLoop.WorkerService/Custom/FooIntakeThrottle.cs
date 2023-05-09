namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeThrottle : IKafkaIntakeThrottle
    {
        private readonly ILogger<FooIntakeThrottle> _logger;

        public FooIntakeThrottle(ILogger<FooIntakeThrottle> logger)
        {
            _logger = logger;
        }

        public async Task ControlSpeedAsync(Func<Task<ThrottleOptions>> manageable, CancellationToken cancellationToken)
        {
            await manageable();

            _logger.LogDebug("Custom throttling...");
            await Task.Delay(5000, cancellationToken);
        }
    }
}
