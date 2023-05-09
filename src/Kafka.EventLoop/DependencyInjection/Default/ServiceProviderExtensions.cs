using Kafka.EventLoop.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection.Default
{
    internal static class ServiceProviderExtensions
    {
        public static T GetOrThrow<T>(
            this IServiceProvider serviceProvider,
            string errorIntro)
            where T : class
        {
            try
            {
                return serviceProvider.GetRequiredService<T>();
            }
            catch (Exception ex)
            {
                throw new DependencyException($"{errorIntro} - failed to get dependency {typeof(T)}", ex);
            }
        }
    }
}
