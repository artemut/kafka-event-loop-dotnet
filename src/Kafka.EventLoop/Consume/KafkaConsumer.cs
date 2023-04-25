using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        private readonly IDeserializer<TMessage> _deserializer;
        private readonly ILogger<KafkaConsumer<TMessage>> _logger;

        public KafkaConsumer(
            IDeserializer<TMessage> deserializer,
            ILogger<KafkaConsumer<TMessage>> logger)
        {
            _deserializer = deserializer;
            _logger = logger;

            _logger.LogInformation($"Consumer uses deserializer: {_deserializer.GetType().FullName}");
        }

        public Task SubscribeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public MessageInfo<TMessage>[] CollectMessages(CancellationToken cancellationToken)
        {
            Task.Delay(new Random().Next(1000, 5000), cancellationToken).Wait(cancellationToken);
            return Array.Empty<MessageInfo<TMessage>>();
        }

        public Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
