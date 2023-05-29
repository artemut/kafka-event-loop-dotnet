using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class StreamConsumerController : IKafkaController<ProductOrderExtendedModel>
    {
        private const string GroupId = "event-loop--stream-consumer-group--2-consumers--fixed-size-5";
        private readonly EventsInterceptor _eventsInterceptor;

        public StreamConsumerController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        public Task ProcessAsync(
            MessageInfo<ProductOrderExtendedModel>[] messages,
            CancellationToken token)
        {
            _eventsInterceptor.ProcessProductOrdersExtendedInvoked(GroupId, messages);
            return Task.CompletedTask;
        }
    }
}
