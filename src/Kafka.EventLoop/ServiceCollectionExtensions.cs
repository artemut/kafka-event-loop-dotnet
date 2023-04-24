using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaEventLoop(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services;
        }
    }
}
