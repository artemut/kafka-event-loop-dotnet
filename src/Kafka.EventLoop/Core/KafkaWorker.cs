using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Core
{
    internal class KafkaWorker<TMessage> : IKafkaWorker
    {
        private readonly string _consumerGroupName;
        private readonly int _consumerId;
        private readonly IIntakeScopeFactory _intakeScopeFactory;

        public KafkaWorker(
            string consumerGroupName,
            int consumerId,
            IIntakeScopeFactory intakeScopeFactory)
        {
            _consumerGroupName = consumerGroupName;
            _consumerId = consumerId;
            _intakeScopeFactory = intakeScopeFactory;
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
