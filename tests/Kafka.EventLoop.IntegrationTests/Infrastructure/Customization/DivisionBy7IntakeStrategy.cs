using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Customization
{
    internal class DivisionBy7IntakeStrategy : IKafkaIntakeStrategy<ProductOrderModel>
    {
        private IKafkaIntakeCancellation? _cancellation;

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
        }

        public void OnNewMessageConsumed(MessageInfo<ProductOrderModel> messageInfo)
        {
            // Cancel when there is a product order ID divisible by 7

            if (messageInfo.Value.Id % 7 == 0)
                _cancellation!.Cancel();
        }
    }
}