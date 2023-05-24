using Confluent.Kafka;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka
{
    internal sealed class IgnoreSerializer : ISerializer<Ignore>
    {
        public byte[] Serialize(Ignore data, SerializationContext context)
        {
            return Array.Empty<byte>();
        }
    }
}
