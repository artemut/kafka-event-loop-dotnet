using Confluent.Kafka;
using System.Text.Json;

namespace Kafka.EventLoop.Conversion
{
    internal sealed class JsonDeserializer<TMessage> : IDeserializer<TMessage?>
    {
        public TMessage? Deserialize(
            ReadOnlySpan<byte> data,
            bool isNull,
            SerializationContext context)
        {
            return isNull ? default : JsonSerializer.Deserialize<TMessage>(data);
        }
    }
}
