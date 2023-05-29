using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class MaxSizeWithTimeoutController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--max-size-timeout-group--3-consumers--size-10-5s";
        private readonly EventsInterceptor _eventsInterceptor;

        public MaxSizeWithTimeoutController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        public Task ProcessAsync(
            MessageInfo<ProductOrderModel>[] messages,
            CancellationToken token)
        {
            _eventsInterceptor.ProcessProductOrdersInvoked(GroupId, messages);
            return Task.CompletedTask;
        }
    }
}
