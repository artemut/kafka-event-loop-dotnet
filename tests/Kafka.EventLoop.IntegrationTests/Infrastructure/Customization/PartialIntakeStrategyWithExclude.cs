using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Customization
{
    internal class PartialIntakeStrategyWithExclude : IKafkaIntakeStrategy<ProductOrderModel>
    {
        private const int PartitionCount = 3;
        private const int TargetMessageCount = 2;
        private readonly Dictionary<int, int> _partitionMessages = Enumerable.Range(0, PartitionCount).ToDictionary(x => x, _ => 0);
        private readonly HashSet<int> _collectedPartitions = new();
        private IKafkaIntakeCancellation? _cancellation;

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
        }

        public void OnNewMessageConsumed(MessageInfo<ProductOrderModel> messageInfo)
        {
            // Wait until we have more than TargetMessageCount messages in each partition
            // but take only TargetMessageCount messages

            _partitionMessages[messageInfo.Partition] += 1;

            if (_partitionMessages[messageInfo.Partition] == TargetMessageCount + 1)
            {
                _collectedPartitions.Add(messageInfo.Partition);

                // Do not include next messages from this partition into the intake
                // Also, exclude the current message
                _cancellation!.StopIntakeForPartition(messageInfo, false);
            }

            if (_collectedPartitions.Count == PartitionCount)
                _cancellation!.Cancel();
        }
    }
}