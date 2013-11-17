using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                OnAfterSendCommand(command, subscribers[0]);
            }
            else
            {
                throw new NoSubscriberRegisteredException(string.Format("No subscriber registered for Commands of type {0}.  {1}", typeof(TCommand), reminderMessage));
            }
        }

        protected virtual void OnAfterSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //no-op
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

        protected virtual TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers) where TEvent : IMessage
        {
            //effectively a no-op unless overridden in derived class
            return @event;
        }

        protected virtual SubscribersResult GetSubscribersFor<TMessage>(TMessage message) where TMessage : IMessage
        {
            List<Action<IMessage>> subscribers;
            return new SubscribersResult(Routes.TryGetValue(message.GetType(), out subscribers), subscribers);
        }

        public virtual void Publish<TEvent>(TEvent @event) where TEvent : IMessage
        {
#if LOG
            Log(string.Format("Entering Publish For EventId={0}, AckId={1}", @event.Id, ((IMessageTx)@event).AcknowledgementId));
#endif
            var subscriberResult = GetSubscribersFor(@event);

            if (!subscriberResult.HasSubscribers) return;

            var subscribers = subscriberResult.Subscribers;

            for (int index = 0; index < subscribers.Count; index++)
            {
#if LOG
                Log(string.Format("About to Prepare EventId={0}, AckId={1}", @event.Id, ((IMessageTx)@event).AcknowledgementId));
#endif
                var preparedEvent = PrepareEventForPublishing(@event, index, subscribers);
#if LOG                
                Log(string.Format("Finished Preparing EventId={0}, AckId={1}", preparedEvent.Id, ((IMessageTx)preparedEvent).AcknowledgementId));
#endif

                if (!ShouldPublishEvent(preparedEvent, index, subscribers)) continue;

                var subscriber = subscribers[index];
                subscriber(preparedEvent);

                OnAfterPublishEvent(preparedEvent, index, subscribers);
            }
        }

        protected void Log(string message)
        {
            Debug.WriteLine(message + "\n");
        }

        protected virtual void OnAfterPublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //no-op
        }

        protected virtual bool ShouldPublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //effectively a no-op unless overridden in derived class
            return true;
        }
    }

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