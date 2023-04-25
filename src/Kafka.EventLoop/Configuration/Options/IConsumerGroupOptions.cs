namespace Kafka.EventLoop.Configuration.Options
{
    public interface IConsumerGroupOptions
    {
        string Name { get; }
        Type MessageType { get; }
        Type MessageDeserializerType { get; }
        Type ControllerType { get; }
        Type? IntakeStrategyType { get; }
        Type? IntakeObserverType { get; }
    }
}
