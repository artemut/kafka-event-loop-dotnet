using Confluent.Kafka;
using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class BarDeadLettersDeserializer : IDeserializer<BarMessage>
    {
        public BarMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
