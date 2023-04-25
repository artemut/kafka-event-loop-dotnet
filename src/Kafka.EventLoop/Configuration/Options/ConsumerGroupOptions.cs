namespace Kafka.EventLoop.Configuration.Options
{
    internal record ConsumerGroupOptions(
        string Name,
        Type MessageType,
        Type MessageDeserializerType,
        Type ControllerType,
        Type? IntakeStrategyType,
        Type? IntakeObserverType) : IConsumerGroupOptions;
}
