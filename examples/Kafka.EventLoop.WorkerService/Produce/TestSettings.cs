namespace Kafka.EventLoop.WorkerService.Produce
{
    public class TestSettings
    {
        public string ConnectionString { get; set; }
        public string FooTopic { get; set; }
        public int FooTopicPartitionCount { get; set; }
        public string BarTopic { get; set; }
        public int BarTopicPartitionCount { get; set; }
        public string BarDeadLettersTopic { get; set; }
        public int BarDeadLettersTopicPartitionCount { get; set; }
        public string FooOneToOneStreamingTopic { get; set; }
        public int FooOneToOneStreamingTopicPartitionCount { get; set; }
    }
}
