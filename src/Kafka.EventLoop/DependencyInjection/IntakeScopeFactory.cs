using Kafka.EventLoop.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.EventLoop.DependencyInjection
{
    internal sealed class IntakeScopeFactory : IIntakeScopeFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IntakeScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IIntakeScope<TMessage> CreateScope<TMessage>(IConsumerGroupOptions consumerGroupOptions)
        {
            return new IntakeScope<TMessage>(_serviceScopeFactory.CreateScope(), consumerGroupOptions);
        }
    }
}
