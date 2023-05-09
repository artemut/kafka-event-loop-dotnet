using Kafka.EventLoop.Configuration.Helpers;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Microsoft.Extensions.Configuration;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop
{
    internal static class Bootstrap
    {
        public static void Configure(
            IConfiguration configuration,
            Func<IKafkaOptionsBuilder, IKafkaOptions> optionsAction,
            IDependencyRegistrar dependencyRegistrar)
        {
            // read config
            var kafkaConfig = ConfigReader.Read(configuration);

            // build options
            var optionsBuilder = new KafkaOptionsBuilder(dependencyRegistrar, kafkaConfig);
            optionsAction(optionsBuilder);

            // register hosted service
            dependencyRegistrar.AddKafkaService(kafkaConfig);
        }
    }
}
