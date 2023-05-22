using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeStrategy : IKafkaIntakeStrategy<FooMessage>
    {
        private IKafkaIntakeCancellation? _cancellation;
        private int _counter;
        
        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
            _cancellation.CancelAfter(TimeSpan.FromSeconds(10));
        }

        public void OnNewMessageConsumed(MessageInfo<FooMessage> messageInfo)
        {
            if (++_counter >= 5)
                _cancellation!.Cancel();
        }
    }
}
