using Kafka.EventLoop.Exceptions;
using Kafka.EventLoop.Produce;

namespace Kafka.EventLoop.Streaming
{
    public abstract class OneToOneStreamingController<TInMessage, TOutMessage>
        : IKafkaStreamingController<TInMessage>, IStreamingInjection<TOutMessage>
    {
        private IKafkaProducer<TOutMessage>? _streamingProducer;
        void IStreamingInjection<TOutMessage>.InjectStreamingProducer(
            IKafkaProducer<TOutMessage> streamingProducer)
        {
            _streamingProducer = streamingProducer;
        }

        public async Task ProcessAsync(
            MessageInfo<TInMessage>[] messages,
            CancellationToken cancellationToken)
        {
            var messageLinks = await BuildOutgoingMessagesAsync(messages, cancellationToken);
            await SendMessagesAsync(messageLinks, cancellationToken);
        }

        protected abstract Task<OneToOneLink<TInMessage, TOutMessage>[]> BuildOutgoingMessagesAsync(
            MessageInfo<TInMessage>[] messages,
            CancellationToken cancellationToken);

        private async Task SendMessagesAsync(
            OneToOneLink<TInMessage, TOutMessage>[] messageLinks,
            CancellationToken cancellationToken)
        {
            if (_streamingProducer == null)
                throw new InvalidOperationException($"Unexpected error: {nameof(_streamingProducer)} is null");

            // we want to send outgoing messages to the same partitions and preserve their order
            var tasks = new List<Task>();
            foreach (var incomingPartitionToItsLinks in messageLinks.GroupBy(x => x.IncomingMessage.Partition))
            {
                var partition = incomingPartitionToItsLinks.Key;
                var outgoingMessages = incomingPartitionToItsLinks.Select(x => x.OutgoingMessage).ToArray();
                var task = _streamingProducer.SendMessagesAsync(outgoingMessages, true, partition, cancellationToken);
                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ProduceException ex)
            {
                throw new ProcessingException(
                    ProcessingErrorCode.TransientError,
                    "An error has occurred while streaming messages to another topic",
                    ex);
            }
        }
    }
}
