using Confluent.Kafka;
using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooMessageDeserializer : IDeserializer<FooMessage>
    {
        public FooMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
