using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        private readonly IConsumer<Ignore, TMessage> _consumer;
        private readonly ILogger<KafkaConsumer<TMessage>> _logger;

        public KafkaConsumer(
            IConsumer<Ignore, TMessage> consumer,
            ILogger<KafkaConsumer<TMessage>> logger)
        {
            _consumer = consumer;
            _logger = logger;
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
