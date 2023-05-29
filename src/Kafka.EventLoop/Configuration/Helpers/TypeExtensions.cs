namespace Kafka.EventLoop.Configuration.Helpers
{
    internal static class TypeExtensions
    {
        public static bool IsKafkaStreamingController(this Type? type)
        {
            if (type == null)
                return false;

            return type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IKafkaStreamingController<>));
        }
    }
}
