using Confluent.Kafka;

namespace Kafka.EventLoop.Exceptions
{
    internal class ConnectivityException : Exception
    {
        public ConnectivityException(string message, KafkaException kafkaException)
            : base(message, kafkaException)
        {
            Error = kafkaException.Error;
        }

        public Error Error { get; }
    }
}
