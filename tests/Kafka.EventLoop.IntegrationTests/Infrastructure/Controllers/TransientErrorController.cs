using Kafka.EventLoop.Exceptions;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class TransientErrorController : IKafkaController<ProductOrderModel>
    {
        private static int _callCount;

        private const string GroupId = "event-loop--transient-error-group--fixed-size-2";
        private readonly EventsInterceptor _eventsInterceptor;

        public TransientErrorController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        public Task ProcessAsync(
            MessageInfo<ProductOrderModel>[] messages,
            CancellationToken token)
        {
            _eventsInterceptor.ProcessProductOrdersInvoked(GroupId, messages);
            if (++_callCount <= 2)
                throw new TransientException();
            return Task.CompletedTask;
        }
    }
}
