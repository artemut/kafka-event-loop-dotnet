namespace Kafka.EventLoop.Exceptions
{
    public class ProduceException : Exception
    {
        public ProduceException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
