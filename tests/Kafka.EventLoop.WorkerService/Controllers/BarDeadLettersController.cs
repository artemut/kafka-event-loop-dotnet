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

        public Task<MessageProcessingResult> ProcessAsync(MessageInfo<BarMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation(
                $"Received {messages.Length} bar dead messages:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, messages.Select(x => x.Value.Key))}");

            return Task.FromResult(MessageProcessingResult.Success);
        }
    }
}
