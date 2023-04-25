using Confluent.Kafka;
using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.DependencyInjection
{
    internal class DependencyRegistry
    {
        public Dictionary<string, Func<IServiceProvider, object>> MessageDeserializerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaControllerProviders { get; } = new();
        public Dictionary<string, ConsumerConfig> ConsumerConfigProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaConsumerFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> IntakeScopeFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, int, IKafkaWorker>> KafkaWorkerFactories { get; } = new();
    }
}
