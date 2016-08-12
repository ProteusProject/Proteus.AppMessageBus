using System;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Portable
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