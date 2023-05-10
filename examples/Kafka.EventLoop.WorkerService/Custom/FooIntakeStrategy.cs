﻿using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeStrategy : IKafkaIntakeStrategy<FooMessage>
    {
        private readonly CancellationTokenSource _cts;
        private int _counter;

        public FooIntakeStrategy()
        {
            _cts = new CancellationTokenSource();
        }

        public CancellationToken Token => _cts.Token;

        public void OnConsumeStarting()
        {
            _cts.CancelAfter(TimeSpan.FromSeconds(10));
        }

        public void OnNewMessageConsumed(MessageInfo<FooMessage> messageInfo)
        {
            if (++_counter >= 5)
                _cts.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
