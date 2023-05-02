using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScope<TMessage> : IIntakeScope<TMessage>
    {
        private readonly IServiceScope _scope;
        private readonly Func<IServiceProvider, IKafkaIntakeStrategy<TMessage>> _intakeStrategyFactory;
        private readonly Func<IServiceProvider, IKafkaIntakeThrottle> _intakeThrottleProvider;
        private readonly Func<IServiceProvider, IKafkaController<TMessage>> _controllerProvider;

        public IntakeScope(
            IServiceScope scope,
            Func<IServiceProvider, IKafkaIntakeStrategy<TMessage>> intakeStrategyFactory,
            Func<IServiceProvider, IKafkaIntakeThrottle> intakeThrottleProvider,
            Func<IServiceProvider, IKafkaController<TMessage>> controllerProvider)
        {
            _scope = scope;
            _intakeStrategyFactory = intakeStrategyFactory;
            _intakeThrottleProvider = intakeThrottleProvider;
            _controllerProvider = controllerProvider;
        }

        public IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy()
        {
            return _intakeStrategyFactory(_scope.ServiceProvider);
        }

        public IKafkaIntakeThrottle GetIntakeThrottle()
        {
            return _intakeThrottleProvider(_scope.ServiceProvider);
        }

        public IKafkaController<TMessage> GetController()
        {
            return _controllerProvider(_scope.ServiceProvider);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
