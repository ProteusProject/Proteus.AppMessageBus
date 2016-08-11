using System;
using System.Collections.Generic;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Portable
{
    public class SubscribersResult
    {
        public Type MessageType { get; private set; }
        public bool HasSubscribers { get; private set; }
        public IList<MessageSubscriber> Subscribers { get; private set; }

        public SubscribersResult(Type messageType, bool hasSubscribers, IList<MessageSubscriber> subscribers)
        {
            MessageType = messageType;
            HasSubscribers = hasSubscribers;
            Subscribers = subscribers;
        }
    }
}