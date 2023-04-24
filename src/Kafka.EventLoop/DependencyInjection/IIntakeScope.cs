namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<TMessage> : IDisposable
    {
        IKafkaController<TMessage> GetController();
    }
}
