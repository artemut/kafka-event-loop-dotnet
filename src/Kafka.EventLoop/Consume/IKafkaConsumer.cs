using Confluent.Kafka;

namespace Kafka.EventLoop.Consume
{
    internal interface IKafkaConsumer<TMessage> : IDisposable
    {
        Task SubscribeAsync(CancellationToken cancellationToken);

        MessageInfo<TMessage>[] CollectMessages(
            IKafkaIntakeStrategy<TMessage> intakeStrategy,
            CancellationToken cancellationToken);

        Task<List<TopicPartition>> GetCurrentAssignmentAsync(CancellationToken cancellationToken);

        Task CommitAsync(MessageInfo<TMessage>[] messages, CancellationToken cancellationToken);

        Task SeekAsync(IDictionary<int, long> partitionToLastAllowedOffset, CancellationToken cancellationToken);

        Task CloseAsync(CancellationToken cancellationToken);
    }
}
