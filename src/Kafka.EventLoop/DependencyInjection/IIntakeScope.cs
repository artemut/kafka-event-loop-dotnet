namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<TMessage> : IDisposable
    {
        IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy();
        IKafkaIntakeThrottle GetIntakeThrottle();
        IKafkaController<TMessage> GetController();
    }
}
