namespace Kafka.EventLoop.Consume
{
    internal interface IKafkaConsumer<TMessage> : IDisposable
    {
        Task SubscribeAsync(CancellationToken cancellationToken);

        MessageInfo<TMessage>[] CollectMessages(CancellationToken cancellationToken);

        Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken);

        Task CloseAsync(CancellationToken cancellationToken);
    }
}
