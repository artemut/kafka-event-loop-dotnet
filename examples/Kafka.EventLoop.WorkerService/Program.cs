using Autofac;
using Autofac.Extensions.DependencyInjection;
using Confluent.Kafka;
using Kafka.EventLoop;
using Kafka.EventLoop.Autofac;
using Kafka.EventLoop.Configuration.Options;
using Kafka.EventLoop.Configuration.OptionsBuilders;
using Kafka.EventLoop.WorkerService;
using Kafka.EventLoop.WorkerService.Controllers;
using Kafka.EventLoop.WorkerService.Custom;
using Kafka.EventLoop.WorkerService.Models;
using Kafka.EventLoop.WorkerService.Produce;
using Microsoft.Extensions.Options;

IKafkaOptions BuildKafkaOptions(IKafkaOptionsBuilder kafkaOptionsBuilder)
{
    return kafkaOptionsBuilder
        .HasConsumerGroup("foo-group", cgOptions => cgOptions
            .HasMessageType<FooMessage>()
            .HasCustomMessageDeserializer<FooMessageDeserializer>()
            .HasController<FooController>()
            .HasCustomIntakeStrategy<FooIntakeStrategy>()
            .HasCustomThrottle<FooIntakeThrottle>()
            .HasStreaming<FooEnrichedMessage>(sOptions => sOptions
                .HasJsonOutMessageSerializer()
                .HasKafkaConfig(c => c.RequestTimeoutMs = 2000)
                .Build())
            .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
            .HasCustomIntakeObserver<FooIntakeObserver>()
            .Build())
        .HasConsumerGroup("bar-group", cgOptions => cgOptions
            .HasMessageType<BarMessage>()
            .HasJsonMessageDeserializer()
            .HasController<BarController>()
            .HasDeadLettering<string>(dlOptions => dlOptions
                .HasDeadLetterMessageKey(x => x.Key)
                .HasJsonDeadLetterMessageSerializer()
                .HasKafkaConfig(c => c.Acks = Acks.All)
                .Build())
            .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
            .Build())
        .HasConsumerGroup("bar-dead-letters-group", cgOptions => cgOptions
            .HasMessageType<BarMessage>()
            .HasCustomMessageDeserializer<BarDeadLettersDeserializer>()
            .HasController<BarDeadLettersController>()
            .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
            .Build())
        .HasConsumerGroup("foo-one-to-one-streaming-group", cgOptions => cgOptions
            .HasMessageType<FooEnrichedMessage>()
            .HasJsonMessageDeserializer()
            .HasController<FooEnrichedController>()
            .HasKafkaConfig(c => c.AutoOffsetReset = AutoOffsetReset.Earliest)
            .Build())
        .Build();
}

var iocContainer = "Autofac"; // "Default" or "Autofac"

var hostBuilder = Host
    .CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(ctx =>
    {
        ctx.AddEnvironmentVariables("KafkaEventLoopTests__");
    });

IHost host;
if (iocContainer == "Default")
{
    host = hostBuilder
        .ConfigureServices((ctx, services) =>
        {
            services.Configure<TestSettings>(ctx.Configuration.GetSection("TestSettings"));
            services.AddHostedService<MessageProduceService>();
            services.AddKafkaEventLoop(ctx.Configuration, BuildKafkaOptions);
        })
        .Build();
}
else if (iocContainer == "Autofac")
{
    host = hostBuilder
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>((ctx, builder) =>
        {
            builder.AddKafkaEventLoop(ctx.Configuration, BuildKafkaOptions);
        })
        .ConfigureServices((ctx, services) =>
        {
            services.Configure<TestSettings>(ctx.Configuration.GetSection("TestSettings"));
            services.AddHostedService<MessageProduceService>();
            services.AddKafkaHostedService();
        })
        .Build();
}
else
{
    throw new InvalidOperationException(iocContainer);
}

var testSetup = new TestKafkaSetup(host.Services.GetRequiredService<IOptions<TestSettings>>());
try
{
    await testSetup.DeleteKafkaTopicsAsync();
    await testSetup.EnsureKafkaTopicsAsync();

    host.Run();
}
finally
{
    await testSetup.DeleteKafkaTopicsAsync();
}