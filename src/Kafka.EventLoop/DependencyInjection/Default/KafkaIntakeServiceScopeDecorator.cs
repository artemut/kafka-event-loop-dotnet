using Kafka.EventLoop.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection.Default
{
    internal class KafkaIntakeServiceScopeDecorator : IKafkaIntake
    {
        private readonly IServiceScope _serviceScope;
        private readonly IKafkaIntake _innerIntake;

        public KafkaIntakeServiceScopeDecorator(IServiceScope serviceScope, IKafkaIntake innerIntake)
        {
            _serviceScope = serviceScope;
            _innerIntake = innerIntake;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _innerIntake.ExecuteAsync(cancellationToken);
        }

        public void Dispose()
        {
            _innerIntake.Dispose();
            _serviceScope.Dispose();
        }
    }
}
