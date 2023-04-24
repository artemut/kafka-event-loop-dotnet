using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly string _consumerGroupName;
        private readonly int _consumerId;
        private readonly IIntakeScopeFactory _intakeScopeFactory;
        private int _isRunning;

        public KafkaWorker(
            string consumerGroupName,
            int consumerId,
            IIntakeScopeFactory intakeScopeFactory)
        {
            _consumerGroupName = consumerGroupName;
            _consumerId = consumerId;
            _intakeScopeFactory = intakeScopeFactory;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                throw new InvalidOperationException(
                    $"Consumer {_consumerGroupName}:{_consumerId} is already running");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = _intakeScopeFactory.CreateScope<TMessage>())
                {
                    var controller = scope.GetController();
                    await controller.ProcessAsync(Array.Empty<TMessage>(), cancellationToken);
                }

                await Task.Delay(1000, cancellationToken);
            }

            _isRunning = 0;
        }
    }
}
