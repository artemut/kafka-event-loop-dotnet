using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Controllers
{
    internal class BarDeadLettersController : IKafkaController<BarMessage>
    {
        private readonly ILogger<BarDeadLettersController> _logger;

        public BarDeadLettersController(ILogger<BarDeadLettersController> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(MessageInfo<BarMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation("\tBarDeadLettersController.ProcessAsync");
            return Task.CompletedTask;
        }
    }
}
