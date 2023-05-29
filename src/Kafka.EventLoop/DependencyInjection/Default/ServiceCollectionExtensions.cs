using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection.Default
{
    internal static class ServiceCollectionExtensions
    {
        public static bool IsRegistered<T>(this IServiceCollection services)
        {
            return services.Any(s => s.ImplementationType == typeof(T) || s.ImplementationInstance is T);
        }
    }
}
