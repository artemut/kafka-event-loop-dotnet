namespace Kafka.EventLoop.Configuration
{
    internal static class Defaults
    {
        public const int SubscribeTimeoutMs = 2000;
        public const int GetCurrentAssignmentTimeoutMs = 2000;
        public const int CommitTimeoutMs = 2000;
        public const int SeekTimeoutMs = 2000;
        public const int CloseTimeoutMs = 2000;

        public const int RestartConsumerAfterMs = 5000;

        public const int RequestTimeoutMs = 2000;
        public const int SocketTimeoutMs = 2000;
        public const int MessageTimeoutMs = 2000;
    }
}
