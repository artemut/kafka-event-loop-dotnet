using Kafka.EventLoop;
using Kafka.EventLoop.WorkerService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>();

        services.AddKafkaEventLoop(ctx.Configuration);
    })
    .Build();

host.Run();