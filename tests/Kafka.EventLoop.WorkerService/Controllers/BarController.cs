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

        public Task<MessageProcessingResult> ProcessAsync(MessageInfo<BarMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation(
                $"Received {messages.Length} bar messages:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, messages.Select(x => x.Value.Key))}");

            return Task.FromResult(MessageProcessingResult.CriticalError);
        }
    }
}
