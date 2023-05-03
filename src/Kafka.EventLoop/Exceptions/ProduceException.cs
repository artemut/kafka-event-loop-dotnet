namespace Kafka.EventLoop.Exceptions
{
    internal class ProduceException : Exception
    {
        public ProduceException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
