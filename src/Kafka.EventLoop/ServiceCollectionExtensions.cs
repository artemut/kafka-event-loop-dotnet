using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Kafka.EventLoop.DependencyInjection.Default;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaEventLoop(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<IKafkaOptionsBuilder, IKafkaOptions> optionsAction)
        {
            var registry = new DependencyRegistry();
            var registrar = new DependencyRegistrar(services, registry);

            Bootstrap.Configure(configuration, optionsAction, registrar);

            return services;
        }
    }
}
