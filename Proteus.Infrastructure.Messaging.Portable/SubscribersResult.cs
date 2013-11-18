using System;
using System.Collections.Generic;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class SubscribersResult
    {
        public bool HasSubscribers { get; private set; }
        public List<Action<IMessage>> Subscribers { get; private set; }

        public SubscribersResult(bool hasSubscribers, List<Action<IMessage>> subscribers)
        {
            HasSubscribers = hasSubscribers;
            Subscribers = subscribers;
        }
    }
}