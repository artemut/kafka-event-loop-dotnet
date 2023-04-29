namespace Kafka.EventLoop
{
    public enum MessageProcessingResult
    {
        Success = 1,
        TransientError = 2,
        CriticalError = 3
    }
}
