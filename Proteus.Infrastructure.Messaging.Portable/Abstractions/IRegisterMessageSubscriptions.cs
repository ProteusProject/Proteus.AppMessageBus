using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IRegisterMessageSubscriptions
    {
        void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage;
        bool HasSubscriptionFor<TMessage>() where TMessage : IMessage;
        void UnRegisterAllSubscriptionsFor<TMessage>() where TMessage : IMessage;
    }
}