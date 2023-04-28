using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeStrategy : IKafkaIntakeStrategy<FooMessage>
    {
        private readonly CancellationTokenSource _cts;

        public FooIntakeStrategy()
        {
            _cts = new CancellationTokenSource();
        }

        public CancellationToken Token => _cts.Token;

        public void OnNewMessageConsumed(MessageInfo<FooMessage> messageInfo)
        {
            // finish intake each time we have a message key which can be divided by 7
            if (messageInfo.Value.Key % 7 == 0)
                _cts.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
