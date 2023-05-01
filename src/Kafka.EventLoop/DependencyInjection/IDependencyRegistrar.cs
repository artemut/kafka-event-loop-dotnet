using Confluent.Kafka;
using Kafka.EventLoop.Configuration.ConfigTypes;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IDependencyRegistrar
    {
        void AddJsonMessageDeserializer<TMessage>(string groupId);
        void AddCustomMessageDeserializer<TDeserializer>(string groupId) where TDeserializer : class;
        void AddFixedSizeIntakeStrategy<TMessage>(string groupId, FixedSizeIntakeStrategyConfig config);
        void AddFixedIntervalIntakeStrategy<TMessage>(string groupId, FixedIntervalIntakeStrategyConfig config);
        void AddMaxSizeWithTimeoutIntakeStrategy<TMessage>(string groupId, MaxSizeWithTimeoutIntakeStrategyConfig config);
        void AddCustomIntakeStrategy<TStrategy>(string groupId) where TStrategy : class;
        void AddDefaultIntakeThrottle(string groupId, IntakeConfig? intakeConfig);
        void AddCustomIntakeThrottle<TThrottle>(string groupId) where TThrottle : class;
        void AddKafkaController<TController>(string groupId) where TController : class;
        void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config);
        void AddKafkaConsumer<TMessage>(string groupId, ConsumerConfig confluentConfig);
        void AddIntakeScope<TMessage>(string groupId);
        void AddDeadLetterMessageKey<TMessageKey, TMessage>(string groupId, Func<TMessage, TMessageKey> messageKeyProvider);
        void AddJsonDeadLetterMessageSerializer<TMessage>(string groupId);
        void AddCustomDeadLetterMessageSerializer<TSerializer>(string groupId) where TSerializer : class;
        void AddDeadLetterProducer<TMessageKey, TMessage>(string groupId, ProduceConfig config, ProducerConfig confluentConfig);
        void AddKafkaWorker<TMessage>(string groupId);
    }
}
