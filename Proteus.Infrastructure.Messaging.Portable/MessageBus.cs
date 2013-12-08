using System;
using System.Collections.Generic;
using System.Diagnostics;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MessageBus : ISendCommands, IPublishEvents, IRegisterMessageSubscriptions
    {
        protected readonly Dictionary<Type, List<Action<IMessage>>> Routes = new Dictionary<Type, List<Action<IMessage>>>();

        public Action<string> Logger { get; set; }

        public virtual void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            Logger(string.Format("Registering Subscriber for Messages of type {0}", typeof(TMessage).AssemblyQualifiedName));

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
            Logger(string.Format("Unregistering all Subscribers for Messages of type {0}", typeof(TMessage).AssemblyQualifiedName));

            if (HasSubscriptionFor<TMessage>())
            {
                Routes.Remove(typeof(TMessage));
            }
        }

        public virtual void Send<TCommand>(TCommand command) where TCommand : ICommand
        {
            Logger(string.Format("Sending Command of type {0}, MessageId = {1}", typeof(TCommand).AssemblyQualifiedName, command.Id));

            const string reminderMessage = "Each Command must have exacty one subscriber registered.";

            List<Action<IMessage>> subscribers;
            if (Routes.TryGetValue(command.GetType(), out subscribers))
            {
                if (subscribers.Count != 1) throw new DuplicateSubscriberRegisteredException(string.Format("There are {0} handlers registered for Commands of type {1}.  {2}", subscribers.Count, typeof(TCommand), reminderMessage));

                OnBeforeSendCommand(command, subscribers[0]);

                command = PrepareCommandForSending(command, subscribers[0]);

                if (!ShouldSendCommand(command, subscribers[0])) return;

                subscribers[0](command);

                OnAfterSendCommand(command, subscribers[0]);
            }
            else
            {
                throw new NoSubscriberRegisteredException(string.Format("No subscriber registered for Commands of type {0}.  {1}", typeof(TCommand), reminderMessage));
            }
        }

        public virtual void Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var subscriberResult = GetSubscribersFor(@event);

            if (!subscriberResult.HasSubscribers) return;

            var subscribers = subscriberResult.Subscribers;

            for (var index = 0; index < subscribers.Count; index++)
            {
                OnBeforePublishEvent(@event, index, subscribers);

                var preparedEvent = PrepareEventForPublishing(@event, index, subscribers);

                if (!ShouldPublishEvent(preparedEvent, index, subscribers)) continue;

                Logger(string.Format("Publishing Event of type {0}, MessageId = {1}, Subscriber Index = {2}", typeof(TEvent).AssemblyQualifiedName, @event.Id, index));

                var subscriber = subscribers[index];
                subscriber(preparedEvent);

                OnAfterPublishEvent(preparedEvent, index, subscribers);
            }
        }

        protected virtual void OnBeforeSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //no-op
        }

        protected virtual void OnAfterSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //no-op
        }

        protected virtual void OnBeforePublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //no-op
        }

        protected virtual void OnAfterPublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //no-op
        }

        protected virtual TCommand PrepareCommandForSending<TCommand>(TCommand command, Action<IMessage> subscribers) where TCommand :IMessage
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

        protected virtual bool ShouldSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //effectively a no-op unless overridden in a derived class
            return true;
        }

        protected virtual bool ShouldPublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            //effectively a no-op unless overridden in derived class
            return true;
        }

        public MessageBus()
        {
            //set null-logger as default unless overridden later
            Logger = (message) => { };
        }
    }
}