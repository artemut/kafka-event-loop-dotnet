using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.Utils;

namespace Kafka.EventLoop.DependencyInjection
{
    internal class DependencyRegistry
    {
        public Dictionary<string, Func<IServiceProvider, object>> MessageDeserializerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaIntakeStrategyFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaIntakeThrottleFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaControllerProviders { get; } = new();
        public Dictionary<string, ConsumerGroupConfig> ConsumerGroupConfigProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> KafkaConsumerFactories { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> IntakeScopeFactories { get; } = new();
        public Dictionary<string, object> DeadLetterMessageKeyProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, object>> DeadLetterMessageSerializerProviders { get; } = new();
        public Dictionary<string, LazyFunc<IServiceProvider, object>> DeadLetterProducerProviders { get; } = new();
        public Dictionary<string, Func<IServiceProvider, int, IKafkaWorker>> KafkaWorkerFactories { get; } = new();
    }
}
