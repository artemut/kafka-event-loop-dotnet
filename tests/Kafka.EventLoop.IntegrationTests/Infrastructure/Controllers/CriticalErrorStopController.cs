using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class CriticalErrorStopController : IKafkaController<ProductOrderModel>
    {
        private const string GroupId = "event-loop--critical-error-stop-group--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public CriticalErrorStopController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        public Task ProcessAsync(
            MessageInfo<ProductOrderModel>[] messages,
            CancellationToken token)
        {
            _eventsInterceptor.ProcessProductOrdersInvoked(GroupId, messages);
            throw new Exception("critical error");
        }
    }
}
