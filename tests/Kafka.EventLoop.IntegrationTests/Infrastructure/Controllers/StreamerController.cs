using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;
using Kafka.EventLoop.Streaming;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Controllers
{
    internal class StreamerController : OneToOneStreamingController<
        ProductOrderModel,
        ProductOrderExtendedModel>
    {
        private const string GroupId = "event-loop--streamer-group--2-consumers--fixed-size-10";
        private readonly IFixture _fixture = new Fixture();
        private readonly EventsInterceptor _eventsInterceptor;

        public StreamerController(EventsInterceptor eventsInterceptor)
        {
            _eventsInterceptor = eventsInterceptor;
            _eventsInterceptor.ControllerCreated(GroupId);
        }

        protected override Task<OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>[]> BuildOutgoingMessagesAsync(
            MessageInfo<ProductOrderModel>[] messages,
            CancellationToken cancellationToken)
        {
            _eventsInterceptor.ProcessProductOrdersInvoked(GroupId, messages);

            var links = messages.Select(Map).ToArray();
            _eventsInterceptor.OneToOneStreamingLinksCreated(GroupId, links);

            return Task.FromResult(links);
        }

        private OneToOneLink<ProductOrderModel, ProductOrderExtendedModel> Map(
            MessageInfo<ProductOrderModel> message)
        {
            var extendedModel = _fixture.Create<ProductOrderExtendedModel>();
            return new OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>(message, extendedModel);
        }
    }
}
