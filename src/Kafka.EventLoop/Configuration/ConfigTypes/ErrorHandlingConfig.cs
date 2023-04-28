namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ErrorHandlingConfig
    {
        public TransientErrorHandlingConfig? Transient { get; set; }
        public CriticalErrorHandlingConfig? Critical { get; set; }
    }
}
