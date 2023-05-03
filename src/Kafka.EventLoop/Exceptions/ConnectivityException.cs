namespace Kafka.EventLoop.Exceptions
{
    internal class ConnectivityException : Exception
    {
        public ConnectivityException(string message, bool isFatal, Exception? innerException = null)
            : base(message, innerException)
        {
            IsFatal = isFatal;
        }

        public bool IsFatal { get; }
    }
}
