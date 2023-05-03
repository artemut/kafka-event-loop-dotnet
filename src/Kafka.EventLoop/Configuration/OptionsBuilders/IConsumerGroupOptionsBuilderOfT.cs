using Confluent.Kafka;
using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IConsumerGroupOptionsBuilder<TMessage>
    {
        IConsumerGroupOptionsBuilder<TMessage> HasJsonMessageDeserializer();

        IConsumerGroupOptionsBuilder<TMessage> HasCustomMessageDeserializer<TDeserializer>()
            where TDeserializer : class, IDeserializer<TMessage?>;

        IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeStrategy<TStrategy>()
            where TStrategy : class, IKafkaIntakeStrategy<TMessage>;

        IConsumerGroupOptionsBuilder<TMessage> HasCustomPartitionMessagesFilter<TFilter>()
            where TFilter : class, IKafkaPartitionMessagesFilter<TMessage>;

        IConsumerGroupOptionsBuilder<TMessage> HasCustomThrottle<TThrottle>()
            where TThrottle : class, IKafkaIntakeThrottle;

        IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : class, IKafkaController<TMessage>;

        IConsumerGroupOptionsBuilder<TMessage> HasDeadLettering<TMessageKey>(
            Func<IDeadLetteringOptionsBuilder<TMessageKey, TMessage>, IDeadLetteringOptions> options);

        IConsumerGroupOptionsBuilder<TMessage> HasStreaming<TOutMessage>(
            Func<IStreamingOptionsBuilder<TMessage, TOutMessage>, IStreamingOptions> options);

        IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeObserver<TObserver>()
            where TObserver : IKafkaIntakeObserver<TMessage>;

        IConsumerGroupOptions Build();
    }
}
