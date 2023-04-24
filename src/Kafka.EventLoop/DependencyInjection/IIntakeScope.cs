namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<in TMessage> : IDisposable
    {
        IKafkaController<TMessage> GetController();
    }
}
