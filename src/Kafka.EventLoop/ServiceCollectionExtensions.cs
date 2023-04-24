﻿using Kafka.EventLoop.Configuration.Helpers;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
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
            // build options
            var optionsBuilder = new KafkaOptionsBuilder();
            var options = optionsAction(optionsBuilder);
            services.AddSingleton(options);

            // read config
            var kafkaConfig = ConfigReader.Read(configuration);
            services.AddSingleton(kafkaConfig);

            // register dependencies in IoC-container
            foreach (var consumerGroupOptions in options.ConsumerGroups)
            {
                services.AddKafkaController(consumerGroupOptions);
            }
            services.AddSingleton<IIntakeScopeFactory, IntakeScopeFactory>();
            services.AddSingleton<IKafkaWorkerFactory, KafkaWorkerFactory>();

            return services;
        }

        private static IServiceCollection AddKafkaController(
            this IServiceCollection services,
            IConsumerGroupOptions consumerGroupOptions)
        {
            var implType = consumerGroupOptions.ControllerType;
            if (services.Any(x => x.ImplementationType == implType))
            {
                // controller of such type is already registered for another consumer group
                return services;
            }

            var messageType = consumerGroupOptions.MessageType;
            var serviceType = TypeResolver.BuildControllerServiceType(messageType);
            services.AddScoped(serviceType, implType);
            return services;
        }
    }
}