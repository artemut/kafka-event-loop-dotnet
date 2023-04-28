namespace Kafka.EventLoop.Exceptions
{
    public class ConnectivityException : Exception
    {
        public ConnectivityException(string message, bool isFatal, Exception? innerException = null)
            : base(message, innerException)
        {
            IsFatal = isFatal;
        }

        public bool IsFatal { get; }
    }
}
