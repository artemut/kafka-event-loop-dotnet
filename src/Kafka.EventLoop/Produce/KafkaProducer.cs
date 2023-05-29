using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Exceptions;

namespace Kafka.EventLoop.Produce
{
    internal class KafkaProducer<TKey, TMessage> : IKafkaProducer<TMessage>
    {
        private readonly IProducer<TKey, TMessage> _producer;
        private readonly Func<TMessage, TKey> _messageKeyProvider;
        private readonly ProduceConfig _produceConfig;

        public KafkaProducer(
            IProducer<TKey, TMessage> producer,
            Func<TMessage, TKey> messageKeyProvider,
            ProduceConfig produceConfig)
        {
            _producer = producer;
            _messageKeyProvider = messageKeyProvider;
            _produceConfig = produceConfig;
        }

        public async Task SendMessagesAsync(
            TMessage[] messages,
            bool sendSequentially,
            int? sendToPartition,
            CancellationToken cancellationToken)
        {
            if (!messages.Any())
            {
                return;
            }

            try
            {
                var messagesToSend = messages
                    .Select(message => new Message<TKey, TMessage>
                    {
                        Key = _messageKeyProvider(message),
                        Value = message
                    })
                    .ToArray();

                if (sendSequentially)
                {
                    await SendMessagesSequentiallyAsync(messagesToSend, sendToPartition, cancellationToken);
                    
                }
                else
                {
                    await SendMessagesInParallelAsync(messagesToSend, sendToPartition, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ProduceException(
                    messages.Length > 1
                        ? $"Wasn't able to send all or some of the {messages.Length} messages to the topic {_produceConfig.TopicName}"
                        : $"Wasn't able to send requested message to the topic {_produceConfig.TopicName}", ex);
            }
        }

        public void Dispose()
        {
            _producer.Dispose();
        }

        private async Task SendMessagesSequentiallyAsync(
            Message<TKey, TMessage>[] messagesToSend,
            int? sendToPartition,
            CancellationToken cancellationToken)
        {
            foreach (var message in messagesToSend)
            {
                await SendOrThrowAsync(message, sendToPartition, null, cancellationToken);
            }
        }

        private async Task SendMessagesInParallelAsync(
            Message<TKey, TMessage>[] messagesToSend,
            int? sendToPartition,
            CancellationToken cancellationToken)
        {
            using var exceptionCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(exceptionCts.Token, cancellationToken);
            await Task.WhenAll(messagesToSend
                .Select(message => SendOrThrowAsync(message, sendToPartition, exceptionCts, linkedCts.Token))
                .ToArray());
        }

        private async Task SendOrThrowAsync(
            Message<TKey, TMessage> message,
            int? sendToPartition,
            CancellationTokenSource? exceptionCts,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = !sendToPartition.HasValue
                    ? await _producer.ProduceAsync(
                        _produceConfig.TopicName, message, cancellationToken)
                    : await _producer.ProduceAsync(
                        new TopicPartition(_produceConfig.TopicName, sendToPartition.Value), message, cancellationToken);
                if (result.Status == PersistenceStatus.NotPersisted)
                {
                    throw new Exception("Message was not persisted");
                }
                if (result.Status == PersistenceStatus.PossiblyPersisted)
                {
                    throw new Exception("Message might not have been persisted");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                exceptionCts?.Cancel();
                throw;
            }
        }
    }
}
