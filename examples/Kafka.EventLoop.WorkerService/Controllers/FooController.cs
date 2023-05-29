using Kafka.EventLoop.Streaming;
using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Controllers
{
    internal class FooController : OneToOneStreamingController<FooMessage, FooEnrichedMessage>
    {
        private readonly ILogger<FooController> _logger;

        public FooController(ILogger<FooController> logger)
        {
            _logger = logger;
        }

        protected override Task<OneToOneLink<FooMessage, FooEnrichedMessage>[]> BuildOutgoingMessagesAsync(
            MessageInfo<FooMessage>[] messages,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Received {messages.Length} foo messages:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, messages.Select(ToDisplayText))}");

            var results = messages
                .Select(m => new OneToOneLink<FooMessage, FooEnrichedMessage>(m, Enrich(m.Value)))
                .ToArray();

            return Task.FromResult(results);
        }

        private FooEnrichedMessage Enrich(FooMessage message)
        {
            return new FooEnrichedMessage
            {
                Key = message.Key,
                Text = message.Text,
                Extra = $"extra {message.Key}"
            };
        }

        private static string ToDisplayText(MessageInfo<FooMessage> message)
        {
            return $"{message.Value.Key}, {message.Value.Text} [{message.Partition}]";
        }
    }
}
