namespace Kafka.EventLoop.Exceptions
{
    public class ProcessingException<TMessage> : Exception
    {
        public ProcessingException(
            ProcessingErrorCode errorCode,
            MessageInfo<TMessage>[] messages,
            Exception? innerException = null)
            : base(null, innerException)
        {
            ErrorCode = errorCode;
            Messages = messages;
        }
        
        public ProcessingErrorCode ErrorCode { get; }
        public MessageInfo<TMessage>[] Messages { get; }
    }
}
