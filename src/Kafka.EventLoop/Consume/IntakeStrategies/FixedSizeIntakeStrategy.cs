namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class FixedSizeIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _size;
        private readonly CancellationTokenSource _cts;
        private int _currentSize;

        public FixedSizeIntakeStrategy(int size)
        {
            _size = size;
            _cts = new CancellationTokenSource();
        }

        public CancellationToken Token => _cts.Token;

        public void OnConsumeStarting()
        {
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
            if (++_currentSize >= _size)
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
