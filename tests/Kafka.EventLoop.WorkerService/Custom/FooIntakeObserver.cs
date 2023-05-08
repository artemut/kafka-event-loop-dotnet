using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeObserver : KafkaIntakeObserver<FooMessage>
    {
        private readonly ILogger<FooIntakeObserver> _logger;

        public FooIntakeObserver(ILogger<FooIntakeObserver> logger)
        {
            _logger = logger;
        }

        public override void OnProcessingFinished()
        {
            _logger.LogInformation("Processing is finished!");
        }
    }
}
