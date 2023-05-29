using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class Parallelism1ConsumerController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--parallelism-group--1-consumer--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public Parallelism1ConsumerController(EventsInterceptor eventsInterceptor)
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
