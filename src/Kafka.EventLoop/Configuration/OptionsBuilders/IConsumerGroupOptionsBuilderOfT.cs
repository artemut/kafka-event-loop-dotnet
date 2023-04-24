using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IConsumerGroupOptionsBuilder<TMessage>
    {
        IConsumerGroupOptionsBuilder<TMessage> HasController<TController>()
            where TController : IKafkaController<TMessage>;

        IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeStrategy<TStrategy>()
            where TStrategy : IKafkaIntakeStrategy<TMessage>;

        IConsumerGroupOptionsBuilder<TMessage> HasCustomIntakeObserver<TObserver>()
            where TObserver : IKafkaIntakeObserver<TMessage>;

        IConsumerGroupOptions Build();
    }
}
