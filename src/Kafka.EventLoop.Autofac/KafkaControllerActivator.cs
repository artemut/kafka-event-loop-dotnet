using Autofac;

namespace Kafka.EventLoop.Autofac
{
    internal class KafkaControllerActivator
    {
        private readonly Dictionary<string, List<Action<IComponentContext, object>>> _groupIdToActivations = new();

        public void Register(string groupId, Action<IComponentContext, object> activation)
        {
            if (!_groupIdToActivations.TryGetValue(groupId, out var activations))
            {
                activations = new List<Action<IComponentContext, object>>();
                _groupIdToActivations[groupId] = activations;
            }
            activations.Add(activation);
        }

        public void Activate(string groupId, IComponentContext context, object instance)
        {
            if (!_groupIdToActivations.TryGetValue(groupId, out var activations))
                return;

            foreach (var activation in activations)
            {
                activation(context, instance);
            }
        }
    }
}
