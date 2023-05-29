namespace Kafka.EventLoop.WorkerService.Models
{
    internal class FooMessage : IMessage<int>
    {
        public int Key { get; set; }
        public string? Text { get; set; }
    }
}
