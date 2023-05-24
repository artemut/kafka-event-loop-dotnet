using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class FixedIntervalStrategyController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--fixed-interval-group--2-consumers--fixed-interval-5s";
        private readonly EventsInterceptor _eventsInterceptor;

        public FixedIntervalStrategyController(EventsInterceptor eventsInterceptor)
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
