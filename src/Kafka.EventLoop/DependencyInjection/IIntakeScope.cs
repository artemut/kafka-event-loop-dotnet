namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<TMessage> : IDisposable
    {
        IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy();
        IKafkaController<TMessage> GetController();
    }
}
