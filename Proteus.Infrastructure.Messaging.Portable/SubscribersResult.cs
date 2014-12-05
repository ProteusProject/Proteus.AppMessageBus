using System;
using System.Collections.Generic;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class SubscribersResult
    {
        public Type MessageType { get; private set; }
        public bool HasSubscribers { get; private set; }
        public List<Action<IMessage>> Subscribers { get; private set; }

        public SubscribersResult(Type messageType, bool hasSubscribers, List<Action<IMessage>> subscribers)
        {
            MessageType = messageType;
            HasSubscribers = hasSubscribers;
            Subscribers = subscribers;
        }
    }
}