using Kafka.EventLoop.Consume.Filtration;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScope<TMessage> : IDisposable
    {
        IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy();
        IKafkaIntakeFilter<TMessage> GetIntakeFilter();
        IKafkaIntakeThrottle GetIntakeThrottle();
        IKafkaController<TMessage> GetController();
    }
}
