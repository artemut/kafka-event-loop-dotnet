namespace Kafka.EventLoop.WorkerService.Models
{
    internal class BarMessage : IMessage<string>
    {
        public string Key { get; set; }
        public string? Text { get; set; }
        public string? Extra { get; set; }
    }
}
