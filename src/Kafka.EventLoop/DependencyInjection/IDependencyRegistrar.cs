using Confluent.Kafka;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IDependencyRegistrar
    {
        void AddJsonMessageDeserializer<TMessage>(string groupId);
        void AddCustomMessageDeserializer<TDeserializer>(string groupId) where TDeserializer : class;
        void AddKafkaController<TController>(string groupId) where TController : class;
        void AddConsumerConfig(string groupId, ConsumerConfig config);
        void AddKafkaConsumer<TMessage>(string groupId);
        void AddIntakeScope<TMessage>(string groupId);
        void AddKafkaWorker<TMessage>(string groupId);
    }
}
