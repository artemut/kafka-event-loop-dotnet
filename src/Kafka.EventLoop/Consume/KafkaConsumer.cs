namespace Kafka.EventLoop.Consume
{
    internal class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>
    {
        public KafkaConsumer()
        {
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
