using Kafka.EventLoop.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.Autofac
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaHostedService(this IServiceCollection services)
        {
            services.AddHostedService(sp => sp.GetRequiredService<KafkaBackgroundService>());
            return services;
        }
    }
}
