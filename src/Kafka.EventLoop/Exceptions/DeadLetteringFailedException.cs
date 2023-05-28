namespace Kafka.EventLoop.Exceptions
{
    internal class DeadLetteringFailedException : Exception
    {
        public DeadLetteringFailedException(Exception innerException)
            : base(null, innerException)
        {
        }
    }
}
