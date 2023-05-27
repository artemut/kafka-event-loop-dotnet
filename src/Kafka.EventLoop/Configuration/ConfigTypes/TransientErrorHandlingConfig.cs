namespace Kafka.EventLoop.Configuration.ConfigTypes
{
    internal class TransientErrorHandlingConfig
    {
        public int? PauseAfterTransientErrorMs { get; set; }
    }
}
