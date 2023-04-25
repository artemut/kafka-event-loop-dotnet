using Kafka.EventLoop.Configuration.Helpers;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Kafka.EventLoop.Core;
using Kafka.EventLoop.DependencyInjection;
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
            // create dependency registry
            var registry = new DependencyRegistry();
            var registrar = new DependencyRegistrar(services, registry);

            // build options
            var optionsBuilder = new KafkaOptionsBuilder(registrar);
            var options = optionsAction(optionsBuilder);
            services.AddSingleton(options);

            // read config
            var kafkaConfig = ConfigReader.Read(configuration);
            services.AddSingleton(kafkaConfig);

            // register hosted service
            services.AddSingleton<Func<string, int, IKafkaWorker>>(
                sp => (name, id) => registry.KafkaWorkerFactories[name](sp, id));
            services.AddHostedService<KafkaBackgroundService>();

            return services;
        }
    }
}
