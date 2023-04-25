namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder : IConsumerGroupOptionsBuilder
    {
        private readonly string _name;

        public ConsumerGroupOptionsBuilder(string name)
        {
            _name = name;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasMessageType<TMessage>()
        {
            return new ConsumerGroupOptionsBuilder<TMessage>(_name);
        }
    }
}
