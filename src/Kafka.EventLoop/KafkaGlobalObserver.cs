namespace Kafka.EventLoop
{
    public abstract class KafkaGlobalObserver
    {
        public virtual void OnConsumerSubscribed(ConsumerId consumerId)
        {
        }
    }
}
