using Autofac;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Microsoft.Extensions.Configuration;

namespace Kafka.EventLoop.Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddKafkaEventLoop(
            this ContainerBuilder builder,
            IConfiguration configuration,
            Func<IKafkaOptionsBuilder, IKafkaOptions> optionsAction)
        {
            var registrar = new AutofacDependencyRegistrar(builder);

            Bootstrap.Configure(configuration, optionsAction, registrar);

            return builder;
        }
    }
}
