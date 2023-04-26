using System.Text.Json;
using Confluent.Kafka;
using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class BarDeadLettersDeserializer : IDeserializer<BarMessage?>
    {
        public BarMessage? Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var message = JsonSerializer.Deserialize<BarMessage>(data);
            if (message != null)
                message.Extra = "custom deserialization";
            return message;
        }
    }
}
