namespace Kafka.EventLoop
{
    public record ThrottleOptions(int AssignedPartitionCount, int ConsumedMessageCount)
    {
        public static ThrottleOptions Empty => new(0, 0);
    }
}
