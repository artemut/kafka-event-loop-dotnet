using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScope<TMessage> : IIntakeScope<TMessage>
    {
        private static readonly Type ControllerType = TypeResolver.BuildControllerServiceType(typeof(TMessage));
        private readonly IServiceScope _scope;

        public IntakeScope(IServiceScope scope)
        {
            _scope = scope;
        }

        public IKafkaController<TMessage> GetController()
        {
            if (_scope.ServiceProvider.GetRequiredService(ControllerType) is not IKafkaController<TMessage> controller)
            {
                throw new InvalidOperationException($"Cannot resolve controller of type {ControllerType.Name}");
            }
            return controller;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
