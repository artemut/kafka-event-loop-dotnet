using Confluent.Kafka;

namespace Kafka.EventLoop.Consume
{
    internal interface IKafkaConsumer<TMessage> : IDisposable
    {
        ConsumerId ConsumerId { get; }

        Task SubscribeAsync(CancellationToken cancellationToken);

        MessageInfo<TMessage>[] CollectMessages(
            IKafkaIntakeStrategy<TMessage> intakeStrategy,
            out bool containsPartialResults,
            CancellationToken cancellationToken);

        Task<List<TopicPartition>> GetCurrentAssignmentAsync(CancellationToken cancellationToken);

        Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken);

        Task SeekAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken);

        Task CloseAsync();
    }
}
