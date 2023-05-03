using Kafka.EventLoop.Configuration.ConfigTypes;

namespace Kafka.EventLoop.Exceptions
{
    internal class DeadLetteringFailedException : Exception
    {
        public DeadLetteringFailedException(DeadLetteringFailStrategy? strategy, Exception innerException)
            : base(null, innerException)
        {
            Strategy = strategy;
        }

        public DeadLetteringFailStrategy? Strategy { get; }
    }
}
