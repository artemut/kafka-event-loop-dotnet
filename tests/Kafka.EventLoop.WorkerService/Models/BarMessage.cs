namespace Kafka.EventLoop.WorkerService.Models
{
    internal class BarMessage
    {
        public string Key { get; set; }
        public string? Text { get; set; }
        public string? Extra { get; set; }
    }
}
