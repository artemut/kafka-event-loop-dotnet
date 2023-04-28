using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScope<TMessage> : IIntakeScope<TMessage>
    {
        private readonly IServiceScope _scope;
        private readonly Func<IServiceProvider, IKafkaIntakeStrategy<TMessage>> _intakeStrategyFactory;
        private readonly Func<IServiceProvider, IKafkaIntakeThrottle> _intakeThrottleFactory;
        private readonly Func<IServiceProvider, IKafkaController<TMessage>> _controllerProvider;

        public IntakeScope(
            IServiceScope scope,
            Func<IServiceProvider, IKafkaIntakeStrategy<TMessage>> intakeStrategyFactory,
            Func<IServiceProvider, IKafkaIntakeThrottle> intakeThrottleFactory,
            Func<IServiceProvider, IKafkaController<TMessage>> controllerProvider)
        {
            _scope = scope;
            _intakeStrategyFactory = intakeStrategyFactory;
            _intakeThrottleFactory = intakeThrottleFactory;
            _controllerProvider = controllerProvider;
        }

        public IKafkaIntakeStrategy<TMessage> CreateIntakeStrategy()
        {
            return _intakeStrategyFactory(_scope.ServiceProvider);
        }

        public IKafkaIntakeThrottle CreateIntakeThrottle()
        {
            return _intakeThrottleFactory(_scope.ServiceProvider);
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
