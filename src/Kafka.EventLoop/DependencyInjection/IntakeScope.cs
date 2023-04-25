using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScope<TMessage> : IIntakeScope<TMessage>
    {
        private readonly IServiceScope _scope;
        private readonly Func<IServiceProvider, IKafkaController<TMessage>> _controllerProvider;

        public IntakeScope(
            IServiceScope scope,
            Func<IServiceProvider, IKafkaController<TMessage>> controllerProvider)
        {
            _scope = scope;
            _controllerProvider = controllerProvider;
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
