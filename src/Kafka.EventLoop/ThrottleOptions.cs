namespace Kafka.EventLoop
{
    public record ThrottleOptions(int AssignedPartitionCount, int ConsumedMessageCount);
}
