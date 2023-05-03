using Kafka.EventLoop.Produce;

namespace Kafka.EventLoop.Streaming
{
    internal interface IStreamingInjection<TOutMessage>
    {
        void InjectStreamingProducer(IKafkaProducer<TOutMessage> streamingProducer);
    }
}
