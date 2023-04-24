using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IKafkaOptionsBuilder
    {
        IKafkaOptionsBuilder HasConsumerGroup(
            string name,
            Func<IConsumerGroupOptionsBuilder, IConsumerGroupOptions> options);

        IKafkaOptions Build();
    }
}
