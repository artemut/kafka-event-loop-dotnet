namespace Kafka.EventLoop.WorkerService.Models
{
    internal class FooEnrichedMessage : FooMessage
    {
        public string? Extra { get; set; }
    }
}
