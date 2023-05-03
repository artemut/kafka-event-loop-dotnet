using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Controllers
{
    internal class FooEnrichedController : IKafkaController<FooEnrichedMessage>
    {
        private readonly ILogger<FooEnrichedController> _logger;

        public FooEnrichedController(ILogger<FooEnrichedController> logger)
        {
            _logger = logger;
        }

        public Task ProcessAsync(MessageInfo<FooEnrichedMessage>[] messages, CancellationToken token)
        {
            _logger.LogInformation(
                $"Received {messages.Length} foo enriched messages:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, messages.Select(ToDisplayText))}");

            return Task.CompletedTask;
        }

        private static string ToDisplayText(MessageInfo<FooEnrichedMessage> message)
        {
            return $"{message.Value.Key}, {message.Value.Text}, {message.Value.Extra} [{message.Partition}]";
        }
    }
}
