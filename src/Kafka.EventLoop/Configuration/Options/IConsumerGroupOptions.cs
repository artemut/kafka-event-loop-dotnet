namespace Kafka.EventLoop.Configuration.Options
{
    public interface IConsumerGroupOptions
    {
        string Name { get; }
        Type MessageType { get; }
        SerializationType MessageSerializationType { get; }
        bool IgnoreMessageExtraElements { get; }
        Type ControllerType { get; }
        Type? IntakeStrategyType { get; }
        Type? IntakeObserverType { get; }
    }
}
