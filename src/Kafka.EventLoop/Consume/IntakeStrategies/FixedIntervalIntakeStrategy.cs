namespace Kafka.EventLoop.Consume.IntakeStrategies
{
    internal class FixedIntervalIntakeStrategy<TMessage> : IKafkaIntakeStrategy<TMessage>
    {
        private readonly int _intervalInMs;
        private IKafkaIntakeCancellation? _cancellation;

        public FixedIntervalIntakeStrategy(int intervalInMs)
        {
            _intervalInMs = intervalInMs;
        }

        public void OnConsumeStarting(IKafkaIntakeCancellation cancellation)
        {
            _cancellation = cancellation;
            _cancellation.CancelAfter(_intervalInMs);
        }

        public void OnNewMessageConsumed(MessageInfo<TMessage> messageInfo)
        {
        }
    }
}
