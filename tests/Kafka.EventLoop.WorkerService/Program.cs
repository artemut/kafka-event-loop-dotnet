using Kafka.EventLoop;
using Kafka.EventLoop.WorkerService;
using Kafka.EventLoop.WorkerService.Controllers;
using Kafka.EventLoop.WorkerService.Custom;
using Kafka.EventLoop.WorkerService.Models;
using Kafka.EventLoop.WorkerService.Produce;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(ctx =>
    {
        ctx.AddEnvironmentVariables("KafkaEventLoopTests__");
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<TestSettings>(ctx.Configuration.GetSection("TestSettings"));
        services.AddHostedService<MessageProduceService>();

        services.AddKafkaEventLoop(
            ctx.Configuration,
            options => options
                .HasConsumerGroup("foo-group", cgOptions => cgOptions
                    .HasMessageType<FooMessage>()
                    .HasCustomMessageDeserializer<FooMessageDeserializer>()
                    .HasController<FooController>()
                    .HasCustomIntakeStrategy<FooIntakeStrategy>()
                    .HasCustomPartitionMessagesFilter<FooPartitionMessagesFilter>()
                    .HasCustomThrottle<FooIntakeThrottle>()
                    .HasStreaming<FooEnrichedMessage>(sOptions => sOptions
                        .HasJsonOutMessageSerializer()
                        .Build())
                    .HasCustomIntakeObserver<FooIntakeObserver>()
                    .Build())
                .HasConsumerGroup("bar-group", cgOptions => cgOptions
                    .HasMessageType<BarMessage>()
                    .HasJsonMessageDeserializer()
                    .HasController<BarController>()
                    .HasDeadLettering<string>(dlOptions => dlOptions
                        .HasDeadLetterMessageKey(x => x.Key)
                        .HasJsonDeadLetterMessageSerializer()
                        .Build())
                    .Build())
                .HasConsumerGroup("bar-dead-letters-group", cgOptions => cgOptions
                    .HasMessageType<BarMessage>()
                    .HasCustomMessageDeserializer<BarDeadLettersDeserializer>()
                    .HasController<BarDeadLettersController>()
                    .Build())
                .HasConsumerGroup("foo-one-to-one-streaming-group", cgOptions => cgOptions
                    .HasMessageType<FooEnrichedMessage>()
                    .HasJsonMessageDeserializer()
                    .HasController<FooEnrichedController>()
                    .Build())
                .Build());
    })
    .Build();

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