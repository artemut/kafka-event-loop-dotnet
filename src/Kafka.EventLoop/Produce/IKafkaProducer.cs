namespace Kafka.EventLoop.Produce
{
    internal interface IKafkaProducer<in TMessage> : IDisposable
    {
        Task SendMessagesAsync(
            TMessage[] messages,
            bool sendSequentially,
            CancellationToken cancellationToken);
    }
}
