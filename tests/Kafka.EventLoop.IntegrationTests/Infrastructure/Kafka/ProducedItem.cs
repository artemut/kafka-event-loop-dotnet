using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka
{
    internal record ProducedItem(int Partition, ProductOrderModel ProductOrder, DateTime ProducedAt);
}
