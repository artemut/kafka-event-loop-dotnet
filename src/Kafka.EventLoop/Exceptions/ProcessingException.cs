namespace Kafka.EventLoop.Exceptions
{
    public class ProcessingException : Exception
    {
        public ProcessingException(
            ProcessingErrorCode errorCode,
            string? message = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ProcessingErrorCode ErrorCode { get; }
    }
}
