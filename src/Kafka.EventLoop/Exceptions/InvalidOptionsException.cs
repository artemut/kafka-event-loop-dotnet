namespace Kafka.EventLoop.Exceptions
{
    internal class InvalidOptionsException : Exception
    {
        public InvalidOptionsException(string message) : base(message)
        {
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Message}";
        }
    }
}
