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
            foreach (var messageInfo in messages)
            {
                _logger.LogInformation($"Processing bar dead message with key {messageInfo.Value.Key} ({messageInfo.Value.Extra})");
            }
            return Task.CompletedTask;
        }
    }
}
