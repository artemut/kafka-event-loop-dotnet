using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Controllers
{
    internal class BarController : IKafkaController<BarMessage>
    {
        private readonly ILogger<BarController> _logger;

        public BarController(ILogger<BarController> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(MessageInfo<BarMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation("\tBarController.ProcessAsync");
            return Task.CompletedTask;
        }
    }
}
