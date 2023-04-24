namespace Kafka.EventLoop.Configuration.Options
{
    internal record ConsumerGroupOptions(
        string Name,
        Type MessageType,
        SerializationType MessageSerializationType,
        bool IgnoreMessageExtraElements,
        Type ControllerType,
        Type? IntakeStrategyType,
        Type? IntakeObserverType) : IConsumerGroupOptions;
}
