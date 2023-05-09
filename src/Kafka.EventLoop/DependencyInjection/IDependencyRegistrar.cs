using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IDependencyRegistrar
    {
        void AddJsonMessageDeserializer<TMessage>(string groupId);
        void AddCustomMessageDeserializer<TDeserializer, TMessage>(string groupId)
            where TDeserializer : class, IDeserializer<TMessage?>;
        void AddCustomIntakeObserver<TObserver, TMessage>(string groupId)
            where TObserver : KafkaIntakeObserver<TMessage>;
        void AddFixedSizeIntakeStrategy<TMessage>(string groupId, FixedSizeIntakeStrategyConfig config);
        void AddFixedIntervalIntakeStrategy<TMessage>(string groupId, FixedIntervalIntakeStrategyConfig config);
        void AddMaxSizeWithTimeoutIntakeStrategy<TMessage>(string groupId, MaxSizeWithTimeoutIntakeStrategyConfig config);
        void AddCustomIntakeStrategy<TStrategy, TMessage>(string groupId)
            where TStrategy : class, IKafkaIntakeStrategy<TMessage>;
        void AddCustomPartitionMessagesFilter<TFilter, TMessage>(string groupId)
            where TFilter : class, IKafkaPartitionMessagesFilter<TMessage>;
        void AddDefaultIntakeThrottle(string groupId, IntakeConfig? intakeConfig);
        void AddCustomIntakeThrottle<TThrottle>(string groupId)
            where TThrottle : class, IKafkaIntakeThrottle;
        void AddKafkaController<TController, TMessage>(string groupId)
            where TController : class, IKafkaController<TMessage>;
        void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config);
        void AddKafkaConsumer<TMessage>(string groupId, ConsumerConfig confluentConfig);
        void AddDeadLetterMessageKey<TMessageKey, TMessage>(string groupId, Func<TMessage, TMessageKey> messageKeyProvider);
        void AddJsonDeadLetterMessageSerializer<TMessage>(string groupId);
        void AddCustomDeadLetterMessageSerializer<TSerializer, TMessage>(string groupId)
            where TSerializer : class, ISerializer<TMessage>;
        void AddDeadLetterProducer<TMessageKey, TMessage>(string groupId, ProduceConfig config, ProducerConfig confluentConfig);
        void AddJsonStreamingMessageSerializer<TOutMessage>(string groupId);
        void AddCustomStreamingMessageSerializer<TSerializer, TOutMessage>(string groupId)
            where TSerializer : class, ISerializer<TOutMessage>;
        void AddStreamingProducer<TOutMessage>(string groupId, ProduceConfig config, ProducerConfig confluentConfig);
        void AddKafkaWorker<TMessage>(string groupId);
        void AddKafkaService(KafkaConfig kafkaConfig);
    }
}
