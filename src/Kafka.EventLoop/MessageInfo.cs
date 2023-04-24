namespace Kafka.EventLoop
{
    public record MessageInfo<TMessage>
    {
        internal MessageInfo(TMessage value, DateTime eventTimeUtc, string topic, int partition, long offset)
        {
            Value = value;
            EventTimeUtc = eventTimeUtc;
            Topic = topic;
            Partition = partition;
            Offset = offset;
        }

        public TMessage Value { get; }
        public DateTime EventTimeUtc { get; }
        public string Topic { get; }
        public int Partition { get; }
        public long Offset { get; }
    }
}
