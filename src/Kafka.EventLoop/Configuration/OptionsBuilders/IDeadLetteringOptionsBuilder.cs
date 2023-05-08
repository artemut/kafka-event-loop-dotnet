using Confluent.Kafka;
using Kafka.EventLoop.Configuration.Options;
using System.Linq.Expressions;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IDeadLetteringOptionsBuilder<TMessageKey, TMessage>
    {
        IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasDeadLetterMessageKey(
            Expression<Func<TMessage, TMessageKey>> messageKey);

        IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasJsonDeadLetterMessageSerializer();

        IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasCustomDeadLetterMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TMessage>;

        IDeadLetteringOptionsBuilder<TMessageKey, TMessage> HasKafkaConfig(Action<ProducerConfig> kafkaConfig);

        IDeadLetteringOptions Build();
    }
}
