using Autofac;
using Kafka.EventLoop.Core;

namespace Kafka.EventLoop.Autofac
{
    internal class KafkaIntakeLifetimeScopeDecorator : IKafkaIntake
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IKafkaIntake _innerIntake;

        public KafkaIntakeLifetimeScopeDecorator(ILifetimeScope lifetimeScope, IKafkaIntake innerIntake)
        {
            _lifetimeScope = lifetimeScope;
            _innerIntake = innerIntake;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _innerIntake.ExecuteAsync(cancellationToken);
        }

        public void Dispose()
        {
            _innerIntake.Dispose();
            _lifetimeScope.Dispose();
        }
    }
}
