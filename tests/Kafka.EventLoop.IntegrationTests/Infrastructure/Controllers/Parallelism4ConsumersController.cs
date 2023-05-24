using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class Parallelism4ConsumersController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--parallelism-group--4-consumers--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public Parallelism4ConsumersController(EventsInterceptor eventsInterceptor)
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
