using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.DependencyInjection
{
    internal static class TypeResolver
    {
        public static Type BuildWorkerType(Type messageType)
        {
            return typeof(KafkaWorker<>).MakeGenericType(messageType);
        }

        public static Type BuildControllerServiceType(Type messageType)
        {
            return typeof(IKafkaController<>).MakeGenericType(messageType);
        }
    }
}
