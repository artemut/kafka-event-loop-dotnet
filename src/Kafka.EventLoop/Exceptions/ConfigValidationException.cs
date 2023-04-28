namespace Kafka.EventLoop.Exceptions
{
    public class ConfigValidationException : Exception
    {
        public ConfigValidationException(string propertyName, string? message = null) : base(message)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }

        public override string ToString()
        {
            return $"{Message} [{PropertyName}]";
        }
    }
}
