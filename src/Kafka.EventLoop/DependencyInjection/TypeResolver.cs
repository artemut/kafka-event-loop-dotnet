using Kafka.EventLoop.Consume;
using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.DependencyInjection
{
    internal static class TypeResolver
    {
        public static Type BuildWorkerType(Type messageType)
        {
            return typeof(KafkaWorker<>).MakeGenericType(messageType);
        }

        public static Type BuildConsumerServiceType(Type messageType)
        {
            return typeof(IKafkaConsumer<>).MakeGenericType(messageType);
        }

        public static Type BuildConsumerImplType(Type messageType)
        {
            return typeof(KafkaConsumer<>).MakeGenericType(messageType);
        }
    }
}
