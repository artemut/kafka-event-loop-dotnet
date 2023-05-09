using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.DependencyInjection.Default
{
    internal class DependencyRegistry
    {
        public Dictionary<string, Func<IServiceProvider, object>> MessageDeserializerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object, object>> KafkaIntakeFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaIntakeObserverFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaIntakeStrategyFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaPartitionMessagesFilterProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaIntakeThrottleProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaControllerProviders { get; } = new();
        public Dictionary<string, ConsumerGroupConfig> ConsumerGroupConfigProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, ConsumerId, object>> KafkaConsumerFactories { get; } = new();
        public Dictionary<string, object> DeadLetterMessageKeyProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> DeadLetterMessageSerializerProviders { get; } = new();
        public Dictionary<string, LazyFunc<IServiceProvider, object>> DeadLetterProducerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> StreamingMessageSerializerProviders { get; } = new();
        public Dictionary<string, LazyFunc<IServiceProvider, object>> StreamingProducerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, ConsumerId, IKafkaWorker>> KafkaWorkerFactories { get; } = new();
    }
}
