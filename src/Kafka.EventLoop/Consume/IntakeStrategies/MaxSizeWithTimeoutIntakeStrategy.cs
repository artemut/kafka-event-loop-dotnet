namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class MaxSizeWithTimeoutIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _maxSize;
        private readonly int _timeoutInMs;
        private IKafkaIntakeCancellation? _cancellation;
        private int _currentSize;

        public MaxSizeWithTimeoutIntakeStrategy(int maxSize, int timeoutInMs)
        {
            _maxSize = maxSize;
            _timeoutInMs = timeoutInMs;
        }

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
            _cancellation.CancelAfter(_timeoutInMs);
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
            if (++_currentSize >= _maxSize)
            {
                _cancellation!.Cancel();
            }
        }
    }
}
