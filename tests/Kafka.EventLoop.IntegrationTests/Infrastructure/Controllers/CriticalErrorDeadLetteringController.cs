using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class CriticalErrorDeadLetteringController : IKafkaController<ProductOrderModel>
    {
        private static bool _called;

        private const string GroupId = "event-loop--critical-error-dead-lettering-group--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public CriticalErrorDeadLetteringController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        public Task ProcessAsync(
            MessageInfo<ProductOrderModel>[] messages,
            CancellationToken token)
        {
            _eventsInterceptor.ProcessProductOrdersInvoked(GroupId, messages);
            if (!_called)
            {
                _called = true;
                throw new Exception("critical error");
            }
            return Task.CompletedTask;
        }
    }
}
