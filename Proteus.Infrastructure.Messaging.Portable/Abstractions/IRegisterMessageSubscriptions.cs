using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IRegisterMessageSubscriptions
    {
        void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage;
        void RegisterSubscriptionFor<TMessage>(string subscriberKey, Action<TMessage> handler) where TMessage : IMessage;
        bool HasSubscriptionFor<TMessage>() where TMessage : IMessage;
        bool HasSubscription(string subscriptionKey);
        void UnRegisterAllSubscriptionsFor<TMessage>() where TMessage : IMessage;
        void UnRegisterSubscription(string subscriberKey);
    }
}