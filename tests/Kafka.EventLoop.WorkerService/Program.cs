using Kafka.EventLoop;
using Kafka.EventLoop.Configuration;
using Kafka.EventLoop.WorkerService;
using Kafka.EventLoop.WorkerService.Controllers;
using Kafka.EventLoop.WorkerService.Custom;
using Kafka.EventLoop.WorkerService.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();

        services.AddKafkaEventLoop(
            ctx.Configuration,
            options => options
                .HasConsumerGroup("foo-group", cgOptions => cgOptions
                    .HasMessageType<FooMessage>(SerializationType.Json, true)
                    .HasController<FooController>()
                    .HasCustomIntakeStrategy<FooIntakeStrategy>()
                    .HasCustomIntakeObserver<FooIntakeObserver>()
                    .Build())
                .HasConsumerGroup("bar-group", cgOptions => cgOptions
                    .HasMessageType<BarMessage>(SerializationType.Json, true)
                    .HasController<BarController>()
                    .Build())
                .HasConsumerGroup("bar-dead-letters-group", cgOptions => cgOptions
                    .HasMessageType<BarMessage>(SerializationType.Json, true)
                    .HasController<BarDeadLettersController>()
                    .Build())
                .Build());
    })
    .Build();

host.Run();