namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class FixedIntervalIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly CancellationTokenSource _cts;

        public FixedIntervalIntakeStrategy(int intervalInMs)
        {
            _cts = new CancellationTokenSource(intervalInMs);
        }

        public CancellationToken Token => _cts.Token;

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
