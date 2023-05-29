namespace Kafka.EventLoop
{
    public record MessageInfo<TMessage> : MessageInfo
    {
        internal MessageInfo(TMessage value, DateTime eventTimeUtc, string topic, int partition, long offset)
            : base(eventTimeUtc, topic, partition, offset)
        {
            Value = value;
        }

        public TMessage Value { get; }

        public override string ToString()
        {
            return $"{Partition}: {Value}";
        }
    }
}
