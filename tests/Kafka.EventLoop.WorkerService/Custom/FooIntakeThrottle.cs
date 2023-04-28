namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeThrottle : IKafkaIntakeThrottle
    {
        private readonly ILogger<FooIntakeThrottle> _logger;

        public FooIntakeThrottle(ILogger<FooIntakeThrottle> logger)
        {
            _logger = logger;
        }

        public Task WaitAsync(ThrottleOptions options, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Custom throttling...");
            return Task.Delay(5000, cancellationToken);
        }
    }
}
