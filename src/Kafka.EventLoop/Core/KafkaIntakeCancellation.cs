namespace Kafka.EventLoop.Core
{
    internal class KafkaIntakeCancellation : IKafkaIntakeCancellation, IDisposable
    {
        private readonly CancellationTokenSource _intakeCts;
        private readonly CancellationTokenSource _linkedCts;

        public KafkaIntakeCancellation(CancellationToken externalToken)
        {
            _intakeCts = new();
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_intakeCts.Token, externalToken);
        }

        public CancellationToken Token => _linkedCts.Token;

        public bool IsIntakeCancelled => _intakeCts.IsCancellationRequested;

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

        public void Dispose()
        {
            _linkedCts.Dispose();
            _intakeCts.Dispose();
        }
    }
}
