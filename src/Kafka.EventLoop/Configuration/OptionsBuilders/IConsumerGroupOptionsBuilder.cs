﻿namespace Kafka.EventLoop.Configuration.OptionsBuilders
{
    public interface IConsumerGroupOptionsBuilder
    {
        IConsumerGroupOptionsBuilder<TMessage> HasMessageType<TMessage>();
    }
}
