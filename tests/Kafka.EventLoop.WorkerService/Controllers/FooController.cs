using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Controllers
{
    internal class FooController : IKafkaController<FooMessage>
    {
        private readonly ILogger<FooController> _logger;

        public FooController(ILogger<FooController> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(FooMessage[] messages, CancellationToken token)
        {
            _logger.LogInformation("\tFooController.ProcessAsync");
            return Task.CompletedTask;
        }
    }
}
