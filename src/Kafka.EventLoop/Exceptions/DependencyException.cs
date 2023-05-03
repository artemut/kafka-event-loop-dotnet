namespace Kafka.EventLoop.Exceptions
{
    internal class DependencyException : Exception
    {
        public DependencyException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
