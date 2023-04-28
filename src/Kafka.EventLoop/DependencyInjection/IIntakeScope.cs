namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<TMessage> : IDisposable
    {
        IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy();
        IKafkaIntakeThrottle CreateIntakeThrottle();
        IKafkaController<TMessage> GetController();
    }
}
