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

        public Task<MessageProcessingResult> ProcessAsync(MessageInfo<FooMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation(
                $"Received {messages.Length} foo messages:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, messages.Select(x => x.Value.Key))}");

            return Task.FromResult(MessageProcessingResult.Success);
        }
    }
}
