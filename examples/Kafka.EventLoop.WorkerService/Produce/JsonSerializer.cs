using System.Text.Json;
using Confluent.Kafka;

namespace Kafka.EventLoop.WorkerService.Produce
{
    internal class JsonSerializer<TMessage> : ISerializer<TMessage>
    {
        public byte[] Serialize(TMessage data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
