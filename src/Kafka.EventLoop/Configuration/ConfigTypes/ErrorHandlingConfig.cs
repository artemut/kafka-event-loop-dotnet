namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class ErrorHandlingConfig
    {
        public TransientErrorHandlingConfig Transient { get; set; } = TransientErrorHandlingConfig.Default;
        public CriticalErrorHandlingConfig Critical { get; set; } = CriticalErrorHandlingConfig.Default;

        public static ErrorHandlingConfig Default => new();
    }
}
