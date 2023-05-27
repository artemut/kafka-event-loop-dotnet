namespace Kafka.EventLoop
{
    public abstract record MessageInfo
    {
        internal MessageInfo(DateTime eventTimeUtc, string topic, int partition, long offset)
        {
            EventTimeUtc = eventTimeUtc;
            Topic = topic;
            Partition = partition;
            Offset = offset;
        }
        
        public DateTime EventTimeUtc { get; }
        public string Topic { get; }
        public int Partition { get; }
        public long Offset { get; }
    }
}
