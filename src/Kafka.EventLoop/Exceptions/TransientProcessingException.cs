namespace Kafka.EventLoop.Exceptions
{
    internal class TransientProcessingException : Exception
    {
        internal TransientProcessingException(
            Exception? innerException = null)
            : base(null, innerException)
        {
        }
    }
}
