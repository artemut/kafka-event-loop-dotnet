namespace Kafka.EventLoop.Exceptions
{
    internal class CriticalProcessingException<TMessage> : Exception
    {
        internal CriticalProcessingException(
            MessageInfo<TMessage>[] messages,
            Exception? innerException = null)
            : base(null, innerException)
        {
            Messages = messages;
        }
        
        public MessageInfo<TMessage>[] Messages { get; }
    }
}
