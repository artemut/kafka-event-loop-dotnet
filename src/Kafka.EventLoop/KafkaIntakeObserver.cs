namespace Kafka.EventLoop
{
    public abstract class KafkaIntakeObserver<TMessage> : IDisposable
    {
        public virtual void OnNothingToProcess()
        {
        }
        public virtual void OnMessagesCollected(MessageInfo<TMessage>[] messages)
        {
        }
        public virtual void OnMessagesFiltered(MessageInfo<TMessage>[] messages)
        {
        }
        public virtual void OnProcessingFinished()
        {
        }
        public virtual void OnProcessingException(Exception exception)
        {
        }
        public virtual void OnDeadLetteringFinished()
        {
        }
        public virtual void OnDeadLetteringFailed(Exception exception)
        {
        }
        public virtual void OnCommitted()
        {
        }
        public virtual void Dispose()
        {
        }
    }
}
