namespace Kafka.EventLoop.Exceptions
{
    public class DependencyException : Exception
    {
        public DependencyException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
