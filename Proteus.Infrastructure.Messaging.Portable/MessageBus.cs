using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class MessageBus : ISendCommands, IPublishEvents, IRegisterMessageSubscriptions
    {
        protected readonly Dictionary<Type, IList<MessageSubscriber>> Routes = new Dictionary<Type, IList<MessageSubscriber>>();
        protected Lazy<string> _messageVersion = new Lazy<string>(() => string.Empty);

        public Action<string> Logger { get; set; }

        public string MessageVersion
        {
            get
            {
                return _messageVersion.Value;
            }
        }

        public Func<string> MessageVersionProvider
        {
            set
            {
                _messageVersion = new Lazy<string>(value);
            }
        }

        public virtual void RegisterSubscriptionFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            string subscriberKey = AutoAssignSubscriberKeyFor(handler);
            RegisterSubscriptionFor(subscriberKey, handler);
        }

        private string AutoAssignSubscriberKeyFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            var candidateKey = typeof(TMessage).Name;

            if (HasSubscriptionFor<TMessage>())
            {

                var suffix = 0;

                while (true)
                {
                    suffix++;
                    IList<MessageSubscriber> subscribers;
                    if (Routes.TryGetValue(typeof(TMessage), out subscribers))
                    {
                        if (subscribers.Any(subsc => subsc.Key == candidateKey))
                        {
                            candidateKey += suffix;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }


            return candidateKey;
        }

        public void RegisterSubscriptionFor<TMessage>(string subscriberKey, Action<TMessage> handler) where TMessage : IMessage
        {
            Logger(string.Format("Registering Subscriber for Messages of type {0} using Key {1}", typeof(TMessage).Name, subscriberKey));

            IList<MessageSubscriber> subscribers;
            if (!Routes.TryGetValue(typeof(TMessage), out subscribers))
            {
                subscribers = new List<MessageSubscriber>();
                Routes.Add(typeof(TMessage), subscribers);
            }
            subscribers.Add(new MessageSubscriber(subscriberKey, DelegateConverter.CastArgument<IMessage, TMessage>(x => handler(x))));
        }

        public virtual bool HasSubscriptionFor<TMessage>() where TMessage : IMessage
        {
            return Routes.ContainsKey(typeof(TMessage));
        }

        public bool HasSubscription(string subscriptionKey)
        {
            return Routes.Select(entry => entry.Value).Any(subscr => subscr.Any(item => item.Key == subscriptionKey));
        }

        public virtual void UnRegisterAllSubscriptionsFor<TMessage>() where TMessage : IMessage
        {
            Logger(string.Format("Unregistering all Subscribers for Messages of type {0}", typeof(TMessage).Name));

            if (HasSubscriptionFor<TMessage>())
            {
                Routes.Remove(typeof(TMessage));
            }
        }

        public void UnRegisterSubscription(string subscriberKey)
        {
            Logger(string.Format("Unregistering Subscriber with Key: {0}", subscriberKey));

            if (HasSubscription(subscriberKey))
            {
                Type foundType = null;
                foreach (var route in Routes)
                {
                    if (route.Value.Any(v => v.Key == subscriberKey))
                    {
                        route.Value.Remove(route.Value.Single(v => v.Key == subscriberKey));
                        break;
                    }
                }
            }
        }

        public virtual async Task Send<TCommand>(TCommand command) where TCommand : ICommand
        {
            Logger(string.Format("Sending Command of type {0}, MessageId = {1}", typeof(TCommand).Name, command.Id));

            const string reminderMessage = "Each Command must have exacty one subscriber registered.";

            IList<MessageSubscriber> subscribers;
            if (Routes.TryGetValue(command.GetType(), out subscribers))
            {
                if (subscribers.Count != 1) throw new DuplicateSubscriberRegisteredException(string.Format("There are {0} handlers registered for Commands of type {1}.  {2}", subscribers.Count, typeof(TCommand), reminderMessage));

                var subscriber = subscribers[0];

                OnBeforeSendCommand(command, subscriber.Handler);

                command = PrepareCommandForSending(command, subscriber.Handler);

                if (!ShouldSendCommand(command, subscriber.Handler)) return;

                if (subscriber.Handler.CanBeAwaited())
                {
                    await Task.Run(() => subscriber.Handler(command));
                }
                else
                {
                    subscriber.Handler(command);
                }

                OnAfterSendCommand(command, subscriber.Handler);
            }
            else
            {
                throw new NoSubscriberRegisteredException(string.Format("No subscriber registered for Commands of type {0}.  {1}", typeof(TCommand), reminderMessage));
            }
        }

        public virtual async Task Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var subscriberResult = GetSubscribersFor(@event);

            if (!subscriberResult.HasSubscribers) return;

            var subscribers = subscriberResult.Subscribers;

            for (var index = 0; index < subscribers.Count; index++)
            {
                OnBeforePublishEvent(@event, index, subscribers);

                var preparedEvent = PrepareEventForPublishing(@event, index, subscribers);

                if (!ShouldPublishEvent(preparedEvent, index, subscribers)) continue;

                Logger(string.Format("Publishing Event of type {0}, MessageId = {1}, Subscriber Index = {2}", typeof(TEvent).Name, @event.Id, index));

                var subscriber = subscribers[index];

                if (subscriber.CanBeAwaited())
                {
                    await Task.Run(() => subscriber(preparedEvent));
                }
                else
                {
                    subscriber(preparedEvent);
                }

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

        protected virtual TCommand PrepareCommandForSending<TCommand>(TCommand command, Action<IMessage> subscribers) where TCommand : IMessage
        {
            command.Version = MessageVersion;
            return command;
        }

        protected virtual TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers) where TEvent : IMessage
        {
            @event.Version = MessageVersion;
            return @event;
        }

        protected virtual SubscribersResult GetSubscribersFor<TMessage>(TMessage message) where TMessage : IMessage
        {
            IList<MessageSubscriber> subscribers;

            var hasSubscribers = Routes.TryGetValue(message.GetType(), out subscribers);
            var actions = new List<Action<IMessage>>();

            if (hasSubscribers)
            {
                actions = subscribers.Select(s => s.Handler).ToList();
            }

            return new SubscribersResult(typeof(TMessage), hasSubscribers, actions);
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
            //set no-op logger as default
            Logger = (message) => { };
        }
    }
}