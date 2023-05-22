namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class FixedSizeIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _size;
        private IKafkaIntakeCancellation? _cancellation;
        private int _currentSize;

        public FixedSizeIntakeStrategy(int size)
        {
            _size = size;
        }

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
            if (++_currentSize >= _size)
            {
                _cancellation!.Cancel();
            }
        }
    }
}
