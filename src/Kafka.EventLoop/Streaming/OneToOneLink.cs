namespace Kafka.EventLoop.Streaming
{
    public record OneToOneLink<TInMessage, TOutMessage>(
        MessageInfo<TInMessage> IncomingMessage,
        TOutMessage OutgoingMessage);
}
