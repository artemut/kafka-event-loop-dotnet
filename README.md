# Kafka.EventLoop

Use this library in your .NET Worker Service to have continuous processing of Kafka messages.

The library implements the "Event Loop" design pattern:
- consumes messages from Kafka,
- accumulates them until a certain condition is met,
- sends accumulated messages to your controller,
- waits until your controller is done with message processing,
- commits offsets to Kafka,
- consumes the next messages from Kafka, and so on.

It is built on top of [Confluent's .NET Client for Apache Kafka](https://github.com/confluentinc/confluent-kafka-dotnet) and implements all the necessary infrastructure for message consumption, error handling, throttling, etc. so that you can focus on message processing only.

## Compatibility

Compatible with .NET 6 and higher.

Supports both Microsoft and Autofac IoC containers.

## Features:

Below is a summary of features that the library provides.

For detailed explanation and code examples please follow the [Wiki](https://github.com/artemut/kafka-event-loop-dotnet/wiki).

- **Kafka consuming** - the library primarily functions as a Kafka consumer, utilizing Confluent's Kafka client to handle essential operations. These operations include subscribing to a Kafka topic, consuming messages, committing offsets, participating in a group re-balance, etc.

- **Parallel processing** - you can specify the number of parallel consumers that will consume messages from your topic partitions and send them to your controller in parallel. This will allow you to increase the message throughput.

- **Intake strategies** - you can use different strategies to decide when it is time to stop accumulating messages and send them to your controller for message processing, e.g. "fixed-size", "fixed-interval", etc.

- **Error handling** - there are different ways to react to errors that may occur in your controller during message processing.

- **Dead-lettering** - the library can send messages to a separate topic in case of non-transient errors. This allows you to avoid blocking the main topic from consuming new messages. You can also consume and process "dead" messages in the same Worker Service separately.

- **Throttling** - you can limit the rate at which messages are being consumed and sent to your controller. This might be useful when you want to avoid putting your external components (database, API, etc.) at excessive load during message spikes or when you want to process Kafka messages from previous days, etc.

- **One-to-one streaming** - the library allows you to send X messages to a separate topic "B" for every X consumed messages from the main topic "A" and preserve their original order. This is useful in scenarios when you have "raw" messages in the topic "A" which you want to enrich with additional data and send to the topic "B" for further processing.

## Simplified code example:

* `appsettings.json`:

    ```json
    {
        "Kafka": {
            "ConnectionString": "xxx",
            "ConsumerGroups": [{
                "GroupId": "xxx",
                "TopicName": "xxx",
                "ParallelConsumers": 10,
                "Intake": {
                    "Strategy": {
                        "Name": "FixedInterval",
                        "IntervalInMs": 5000
                    }
                }
            }]
        }
    }
    ```

* `Program.cs`:

    ```csharp
    Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services.AddKafkaEventLoop(ctx.Configuration, o => o
                .HasConsumerGroup("xxx", cgOptions => cgOptions
                    .HasMessageType<MyMessage>()
                    .HasJsonMessageDeserializer()
                    .HasController<MyController>()
                    .Build())
                .Build());
        })
        .Build()
        .Run();
    ```

* `MyController.cs`:

    ```csharp
    public class MyController : IKafkaController<MyMessage>
    {
        public Task ProcessAsync(MessageInfo<MyMessage>[] messages, CancellationToken token)
        {
            // process your messages
        }
    }
    ```

*See more examples on [Wiki](https://github.com/artemut/kafka-event-loop-dotnet/wiki)*

## Contributing

You are welcome to contribute or create an issue.

### Current contributors:

- [Artem Utkin](https://github.com/artemut)