using System.Collections.Concurrent;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;
using Kafka.EventLoop.Streaming;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure
{
    internal class EventsInterceptor : KafkaGlobalObserver
    {
        private readonly ConcurrentBag<ConsumerId> _subscribedConsumers = new();
        private readonly ConcurrentDictionary<string, int> _controllerCreatedCounter = new();
        private readonly ConcurrentDictionary<string, List<MessageInfo<ProductOrderModel>>> _receivedProductOrders = new();
        private readonly ConcurrentDictionary<string, List<OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>>> _oneToOneStreamingLinks = new();
        private readonly ConcurrentDictionary<string, List<MessageInfo<ProductOrderExtendedModel>>> _receivedProductOrdersExtended = new();

        public bool AreConsumersSubscribed(ConsumerId[] consumerIds)
        {
            var subscribedConsumerIds = _subscribedConsumers.ToArray();
            return consumerIds.All(c => subscribedConsumerIds.Contains(c));
        }

        public int GetControllerCreatedCount(string groupId)
        {
            return _controllerCreatedCounter.GetValueOrDefault(groupId, 0);
        }

        public MessageInfo<ProductOrderModel>[]? GetReceivedProductOrders(string groupId)
        {
            return _receivedProductOrders!.GetValueOrDefault(groupId, null)?.ToArray();
        }

        public OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>[]? GetReceivedOneToOneStreamingLinks(string groupId)
        {
            return _oneToOneStreamingLinks!.GetValueOrDefault(groupId, null)?.ToArray();
        }

        public MessageInfo<ProductOrderExtendedModel>[]? GetReceivedProductOrdersExtended(string groupId)
        {
            return _receivedProductOrdersExtended!.GetValueOrDefault(groupId, null)?.ToArray();
        }

        public void Reset(string groupId)
        {
            _controllerCreatedCounter.Remove(groupId, out _);
            _receivedProductOrders.Remove(groupId, out _);
            _oneToOneStreamingLinks.Remove(groupId, out _);
            _receivedProductOrdersExtended.Remove(groupId, out _);
        }

        public override void OnConsumerSubscribed(ConsumerId consumerId)
        {
            _subscribedConsumers.Add(consumerId);
        }

        public void ControllerCreated(string groupId)
        {
            _controllerCreatedCounter.AddOrUpdate(
                groupId,
                _ => 1,
                (_, x) => x + 1);
        }

        public void ProcessProductOrdersInvoked(string groupId, MessageInfo<ProductOrderModel>[] messages)
        {
            _receivedProductOrders.AddOrAppend(groupId, messages);
        }

        public void OneToOneStreamingLinksCreated(string groupId, OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>[] links)
        {
            _oneToOneStreamingLinks.AddOrAppend(groupId, links);
        }

        public void ProcessProductOrdersExtendedInvoked(string groupId, MessageInfo<ProductOrderExtendedModel>[] messages)
        {
            _receivedProductOrdersExtended.AddOrAppend(groupId, messages);
        }
    }
}
