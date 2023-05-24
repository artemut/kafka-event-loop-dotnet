using Confluent.Kafka;
using System.Text.Json;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka
{
    internal sealed class JsonSerializer<TMessage> : ISerializer<TMessage>
    {
        public byte[] Serialize(TMessage data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
