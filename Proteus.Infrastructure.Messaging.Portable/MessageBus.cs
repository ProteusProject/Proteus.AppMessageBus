using System;
using System.Collections.Generic;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MessageBus : ISendCommands, IPublishEvents
    {
        private readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();

        public void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            List<Action<IMessage>> subscribers;
            if (!_routes.TryGetValue(typeof(TMessage), out subscribers))
            {
                subscribers = new List<Action<IMessage>>();
                _routes.Add(typeof(TMessage), subscribers);
            }
            subscribers.Add(DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)));
        }

        public bool HasSubscriptionFor<TMessage>() where TMessage : IMessage
        {
            return _routes.ContainsKey(typeof (TMessage));
        }

        public void UnRegisterAllSubscriptionsFor<TMessage>() where TMessage : IMessage
        {
            if (HasSubscriptionFor<TMessage>())
            {
                _routes.Remove(typeof (TMessage));
            }
        }

        public void Send<TCommand>(TCommand command) where TCommand : Command
        {
            const string reminderMessage = "Each Command must have exacty one subscriber registered.";

            List<Action<IMessage>> subscribers;
            if (_routes.TryGetValue(typeof(TCommand), out subscribers))
            {
                if (subscribers.Count != 1) throw new DuplicateSubscriberRegisteredException(string.Format("There are {0} handlers registered for Commands of type {1}.  {2}", subscribers.Count, typeof(TCommand), reminderMessage));
                subscribers[0](command);
            }
            else
            {
                throw new NoSubscriberRegisteredException(string.Format("No subscriber registered for Commands of type {0}.  {1}", typeof(TCommand), reminderMessage));
            }
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            List<Action<IMessage>> subscribers;
            if (!_routes.TryGetValue(@event.GetType(), out subscribers)) return;
            foreach (var subscriber in subscribers)
            {
                //assign to local var to avoid the .net foreach bug
                var subscriber1 = subscriber;
                subscriber1(@event);
            }
        }
    }
}