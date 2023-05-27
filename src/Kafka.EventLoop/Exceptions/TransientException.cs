namespace Kafka.EventLoop.Exceptions
{
    public class TransientException : Exception
    {
        public TransientException(Exception? innerException = null)
            : base(null, innerException)
        {
        }

        public TransientException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
