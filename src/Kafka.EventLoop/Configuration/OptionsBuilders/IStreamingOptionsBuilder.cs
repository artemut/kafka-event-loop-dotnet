using Confluent.Kafka;
using Kafka.EventLoop.Configuration.Options;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IStreamingOptionsBuilder<TInMessage, TOutMessage>
    {
        IStreamingOptionsBuilder<TInMessage, TOutMessage> HasJsonOutMessageSerializer();

        IStreamingOptionsBuilder<TInMessage, TOutMessage> HasCustomOutMessageSerializer<TSerializer>()
            where TSerializer : class, ISerializer<TOutMessage>;

        IStreamingOptions Build();
    }
}
