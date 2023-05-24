using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Customization
{
    internal class DivisionBy7PartialIntakeStrategy : IKafkaIntakeStrategy<ProductOrderModel>
    {
        private const int PartitionCount = 3;
        private readonly HashSet<int> _collectedPartitions = new();
        private IKafkaIntakeCancellation? _cancellation;

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
        }

        public void OnNewMessageConsumed(MessageInfo<ProductOrderModel> messageInfo)
        {
            // Wait until we have product order IDs divisible by 7 from all the partitions

            if (messageInfo.Value.Id % 7 == 0)
            {
                _collectedPartitions.Add(messageInfo.Partition);

                // Do not include next messages from this partition into the intake
                _cancellation!.StopIntakeForPartition(messageInfo.Partition);
            }

            if (_collectedPartitions.Count == PartitionCount)
                _cancellation!.Cancel();
        }
    }
}