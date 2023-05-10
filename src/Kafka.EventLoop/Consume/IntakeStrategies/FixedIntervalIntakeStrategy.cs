namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class FixedIntervalIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _intervalInMs;
        private readonly CancellationTokenSource _cts;

        public FixedIntervalIntakeStrategy(int intervalInMs)
        {
            _intervalInMs = intervalInMs;
            _cts = new CancellationTokenSource();
        }

        public CancellationToken Token => _cts.Token;

        public void OnConsumeStarting()
        {
            _cts.CancelAfter(_intervalInMs);
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
