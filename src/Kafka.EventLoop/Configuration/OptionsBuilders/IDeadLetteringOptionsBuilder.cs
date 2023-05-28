using Confluent.Kafka;
using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IDeadLetteringOptionsBuilder<TMessage>
    {
        IDeadLetteringOptionsBuilder<TMessage> HasJsonDeadLetterMessageSerializer();

        IDeadLetteringOptionsBuilder<TMessage> HasCustomDeadLetterMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TMessage>;

        IDeadLetteringOptionsBuilder<TMessage> HasKafkaConfig(Action<ProducerConfig> kafkaConfig);

        IDeadLetteringOptions Build();
    }
}
