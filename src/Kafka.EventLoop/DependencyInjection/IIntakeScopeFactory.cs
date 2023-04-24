namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScopeFactory
    {
        IIntakeScope<TMessage> CreateScope<TMessage>();
    }
}
