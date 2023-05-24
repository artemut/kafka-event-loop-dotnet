using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class DeadLettersController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--dead-letters-group--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public DeadLettersController(EventsInterceptor eventsInterceptor)
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
