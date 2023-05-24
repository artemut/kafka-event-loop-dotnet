using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class CustomStrategyController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--custom-strategy-group--2-consumers";
        private readonly EventsInterceptor _eventsInterceptor;

        public CustomStrategyController(EventsInterceptor eventsInterceptor)
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
