using System;
using System.Collections.Generic;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MessageBus : ISendCommands, IPublishEvents
    {
        protected readonly Dictionary<Type, List<Action<IMessage>>> Routes = new Dictionary<Type, List<Action<IMessage>>>();

        public virtual void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            List<Action<IMessage>> subscribers;
            if (!Routes.TryGetValue(typeof(TMessage), out subscribers))
            {
                subscribers = new List<Action<IMessage>>();
                Routes.Add(typeof(TMessage), subscribers);
            }
            subscribers.Add(DelegateConverter.CastArgument<IMessage, TMessage>(x => handler(x)));
        }

        public virtual bool HasSubscriptionFor<TMessage>() where TMessage : IMessage
        {
            return Routes.ContainsKey(typeof(TMessage));
        }

        public virtual void UnRegisterAllSubscriptionsFor<TMessage>() where TMessage : IMessage
        {
            if (HasSubscriptionFor<TMessage>())
            {
                Routes.Remove(typeof(TMessage));
            }
        }

        public virtual void Send<TCommand>(TCommand command) where TCommand : IMessage
        {
            const string reminderMessage = "Each Command must have exacty one subscriber registered.";

            List<Action<IMessage>> subscribers;
            if (Routes.TryGetValue(command.GetType(), out subscribers))
            {
                if (subscribers.Count != 1) throw new DuplicateSubscriberRegisteredException(string.Format("There are {0} handlers registered for Commands of type {1}.  {2}", subscribers.Count, typeof(TCommand), reminderMessage));

                command = PrepareCommandForPublishing(command, subscribers[0]);

                if (!ShouldSendCommand(command, subscribers[0])) return;

                subscribers[0](command);
            }
            else
            {
                throw new NoSubscriberRegisteredException(string.Format("No subscriber registered for Commands of type {0}.  {1}", typeof(TCommand), reminderMessage));
            }
        }

        protected virtual bool ShouldSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //effectively a no-op unless overridden in a derived class
            return true;
        }

        protected virtual TCommand PrepareCommandForPublishing<TCommand>(TCommand command, Action<IMessage> subscribers)
        {
            //effectively a no-op unless overridden in derived class
            return command;
        }

        protected virtual TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //effectively a no-op unless overridden in derived class
            return @event;
        }

        public virtual void Publish<TEvent>(TEvent @event) where TEvent : IMessage
        {
            List<Action<IMessage>> subscribers;
            if (!Routes.TryGetValue(@event.GetType(), out subscribers)) return;
            for (int index = 0; index < subscribers.Count; index++)
            {
                @event = PrepareEventForPublishing(@event, index, subscribers);

                if (!ShouldPublishEvent(@event, index, subscribers)) continue;
                
                var subscriber = subscribers[index];
                subscriber(@event);
            }
        }

        protected virtual bool ShouldPublishEvent(IMessage @event, int index, List<Action<IMessage>> subscribers)
        {
            //effectively a no-op unless overridden in derived class
            return true;
        }
    }
}