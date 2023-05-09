namespace Kafka.EventLoop.WorkerService.Models
{
    internal interface IMessage<TKey>
    {
        public TKey Key { get; set; }
    }
}
