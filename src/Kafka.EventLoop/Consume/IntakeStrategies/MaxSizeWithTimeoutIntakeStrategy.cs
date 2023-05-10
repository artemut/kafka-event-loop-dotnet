namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class MaxSizeWithTimeoutIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _maxSize;
        private readonly int _timeoutInMs;
        private readonly CancellationTokenSource _cts;
        private int _currentSize;

        public MaxSizeWithTimeoutIntakeStrategy(int maxSize, int timeoutInMs)
        {
            _maxSize = maxSize;
            _timeoutInMs = timeoutInMs;
            _cts = new CancellationTokenSource();
        }

        public CancellationToken Token => _cts.Token;

        public void OnConsumeStarting()
        {
            _cts.CancelAfter(_timeoutInMs);
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
            if (++_currentSize >= _maxSize)
            {
                _cts.Cancel();
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
