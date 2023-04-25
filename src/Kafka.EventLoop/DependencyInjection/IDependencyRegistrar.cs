namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IDependencyRegistrar
    {
        void AddJsonMessageDeserializer<TMessage>(string consumerGroupName);
        void AddCustomMessageDeserializer<TDeserializer>(string consumerGroupName) where TDeserializer : class;
        void AddKafkaController<TController>(string consumerGroupName) where TController : class;
        void AddKafkaConsumer<TMessage>(string consumerGroupName);
        void AddIntakeScope<TMessage>(string consumerGroupName);
        void AddKafkaWorker<TMessage>(string consumerGroupName);
    }
}
