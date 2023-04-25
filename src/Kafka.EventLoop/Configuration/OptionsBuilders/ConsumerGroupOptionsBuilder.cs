﻿using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.DependencyInjection;

namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    internal sealed class ConsumerGroupOptionsBuilder : IConsumerGroupOptionsBuilder
    {
        private readonly string _groupId;
        private readonly IDependencyRegistrar _dependencyRegistrar;
        private readonly ConsumerGroupConfig _consumerGroupConfig;

        public ConsumerGroupOptionsBuilder(
            string groupId,
            IDependencyRegistrar dependencyRegistrar,
            ConsumerGroupConfig consumerGroupConfig)
        {
            _groupId = groupId;
            _dependencyRegistrar = dependencyRegistrar;
            _consumerGroupConfig = consumerGroupConfig;
        }

        public IConsumerGroupOptionsBuilder<TMessage> HasMessageType<TMessage>()
        {
            return new ConsumerGroupOptionsBuilder<TMessage>(_groupId, _dependencyRegistrar, _consumerGroupConfig);
        }
    }
}
