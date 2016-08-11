using System;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MessageSubscriber
    {
        public MessageSubscriber(string subscriberKey, Action<IMessage> action)
        {
            Key = subscriberKey;
            Handler = action;
        }

        public string Key { get; private set; }
        public Action<IMessage> Handler { get; set; }
    }
}