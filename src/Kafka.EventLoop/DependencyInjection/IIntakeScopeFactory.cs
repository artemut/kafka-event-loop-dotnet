using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.DependencyInjection
{
    internal interface IIntakeScopeFactory
    {
        IIntakeScope<TMessage> CreateScope<TMessage>(IConsumerGroupOptions consumerGroupOptions);
    }
}
