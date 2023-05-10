using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IKafkaOptionsBuilder
    {
        IKafkaOptionsBuilder HasConsumerGroup(
            string groupId,
            Func<IConsumerGroupOptionsBuilder, IConsumerGroupOptions> options);

        IKafkaOptionsBuilder HasCustomKafkaGlobalObserver<TObserver>()
            where TObserver : KafkaGlobalObserver;

        IKafkaOptions Build();
    }
}
