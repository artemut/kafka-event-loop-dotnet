namespace Kafka.EventLoop
{
    public record ConsumerId(string GroupId, int Index)
    {
        public override string ToString() => $"{GroupId}:{Index}";
    }
}
