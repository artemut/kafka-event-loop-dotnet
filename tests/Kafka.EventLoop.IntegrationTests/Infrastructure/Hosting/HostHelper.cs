using Confluent.Kafka;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Customization;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Hosting
{
    internal class HostHelper
    {
        private readonly IHost _host;

        public HostHelper(EventsInterceptor eventsInterceptor)
        {
            _host = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(ctx =>
                {
                    ctx.AddJsonFile("settings.json");
                    ctx.AddEnvironmentVariables("KafkaEventLoopTests__");
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton(eventsInterceptor);
                    services.AddKafkaEventLoop(ctx.Configuration, BuildKafkaOptions);
                })
                .Build();
        }

        public void Start() => Task.Run(() => _host.StartAsync());

        public void Stop() => _host.StopAsync().GetAwaiter().GetResult();

        private static IKafkaOptions BuildKafkaOptions(IKafkaOptionsBuilder kafkaOptionsBuilder)
        {
            return kafkaOptionsBuilder
                .HasConsumerGroup("event-loop--basic-group", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<BasicController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--parallelism-group--1-consumer--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<Parallelism1ConsumerController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--parallelism-group--2-consumers--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<Parallelism2ConsumersController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--parallelism-group--4-consumers--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<Parallelism4ConsumersController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--fixed-size-group--2-consumers--fixed-size-6", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<FixedSizeStrategyController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--fixed-interval-group--2-consumers--fixed-interval-5s", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<FixedIntervalStrategyController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--max-size-timeout-group--3-consumers--size-10-5s", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<MaxSizeWithTimeoutController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--custom-strategy-group--2-consumers", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<CustomStrategyController>()
                    .HasCustomIntakeStrategy<DivisionBy7IntakeStrategy>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--partial-intake-strategy-group--1-consumer", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<PartialIntakeStrategyController>()
                    .HasCustomIntakeStrategy<DivisionBy7PartialIntakeStrategy>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--transient-error-group--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<TransientErrorController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--critical-error-stop-group--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<CriticalErrorStopController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--critical-error-dead-lettering-group--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<CriticalErrorDeadLetteringController>()
                    .HasDeadLettering<long>(dlOptions => dlOptions
                        .HasJsonDeadLetterMessageSerializer()
                        .HasDeadLetterMessageKey(x => x.Id)
                        .Build())
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--dead-letters-group--fixed-size-2", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<DeadLettersController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--throttling-group--2-consumers--fixed-size-50--speed-50", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<ThrottlingController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--streamer-group--2-consumers--fixed-size-10", cgOptions => cgOptions
                    .HasMessageType<ProductOrderModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<StreamerController>()
                    .HasStreaming<ProductOrderExtendedModel>(sOptions => sOptions
                        .HasJsonOutMessageSerializer()
                        .Build())
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasConsumerGroup("event-loop--stream-consumer-group--2-consumers--fixed-size-5", cgOptions => cgOptions
                    .HasMessageType<ProductOrderExtendedModel>()
                    .HasJsonMessageDeserializer()
                    .HasController<StreamConsumerController>()
                    .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
                    .HasKafkaConfig(c => c.FetchWaitMaxMs = 100)
                    .Build())
                .HasCustomKafkaGlobalObserver<EventsInterceptor>()
                .Build();
        }
    }
}
