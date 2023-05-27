namespace Kafka.EventLoop.Core
{
    internal class KafkaIntakeCancellation : IKafkaIntakeCancellation, IDisposable
    {
        private readonly CancellationTokenSource _intakeCts;
        private readonly CancellationTokenSource _linkedCts;
        private readonly HashSet<int> _stoppedPartitions;
        private readonly Dictionary<int, bool> _includeLastMessage;

        public KafkaIntakeCancellation(CancellationToken externalToken)
        {
            _intakeCts = new();
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_intakeCts.Token, externalToken);
            _stoppedPartitions = new HashSet<int>();
            _includeLastMessage = new Dictionary<int, bool>();
        }

        public CancellationToken Token => _linkedCts.Token;

        public bool IsIntakeCancelled => _intakeCts.IsCancellationRequested;

        public HashSet<int> StoppedPartitions => _stoppedPartitions;

        public Dictionary<int, bool> IncludeLastMessage => _includeLastMessage;

        public void CancelAfter(TimeSpan delay)
        {
            _intakeCts.CancelAfter(delay);
        }

        public void CancelAfter(int millisecondsDelay)
        {
            _intakeCts.CancelAfter(millisecondsDelay);
        }

        public void Cancel()
        {
            _intakeCts.Cancel();
        }

        public void StopIntakeForPartition(MessageInfo message, bool include)
        {
            _stoppedPartitions.Add(message.Partition);
            _includeLastMessage.Add(message.Partition, include);
        }

        public void Dispose()
        {
            _linkedCts.Dispose();
            _intakeCts.Dispose();
        }
    }
}
