using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class PartialIntakeStrategyWithExcludeController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--partial-intake-strategy-with-exclude-group--1-consumer";
        private readonly EventsInterceptor _eventsInterceptor;

        public PartialIntakeStrategyWithExcludeController(EventsInterceptor eventsInterceptor)
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
