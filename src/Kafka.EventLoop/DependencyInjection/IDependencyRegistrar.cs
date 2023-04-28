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
        void AddKafkaController<TController>(string groupId) where TController : class;
        void AddConsumerGroupConfig(string groupId, ConsumerGroupConfig config);
        void AddConfluentConsumerConfig(string groupId, ConsumerConfig config);
        void AddKafkaConsumer<TMessage>(string groupId);
        void AddIntakeScope<TMessage>(string groupId);
        void AddKafkaWorker<TMessage>(string groupId);
    }
}
