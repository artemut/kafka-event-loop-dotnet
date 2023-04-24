using Kafka.EventLoop.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScope<TMessage> : IIntakeScope<TMessage>
    {
        private readonly IServiceScope _scope;
        private readonly IConsumerGroupOptions _consumerGroupOptions;

        public IntakeScope(IServiceScope scope, IConsumerGroupOptions consumerGroupOptions)
        {
            _scope = scope;
            _consumerGroupOptions = consumerGroupOptions;
        }

        public IKafkaController<TMessage> GetController()
        {
            var type = _consumerGroupOptions.ControllerType;
            if (_scope.ServiceProvider.GetRequiredService(type) is not IKafkaController<TMessage> controller)
            {
                throw new InvalidOperationException($"Cannot resolve controller of type {type.Name}");
            }
            return controller;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
