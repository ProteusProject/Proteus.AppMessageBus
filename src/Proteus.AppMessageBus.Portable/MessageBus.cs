using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Portable
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
            subscribers.Add(new MessageSubscriber(subscriberKey, x => handler((TMessage)x)));
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

            const string reminderMessage = "Each Command must have exactly one subscriber registered.";

            IList<MessageSubscriber> subscribers;
            if (Routes.TryGetValue(command.GetType(), out subscribers))
            {
                if (subscribers.Count != 1) throw new DuplicateSubscriberRegisteredException(string.Format("There are {0} handlers registered for Commands of type {1}.  {2}", subscribers.Count, typeof(TCommand), reminderMessage));

                var subscriber = subscribers[0];

                OnBeforeSendCommand(command, subscriber);

                command = PrepareCommandForSending(command, subscriber);

                if (!ShouldSendCommand(command, subscriber.Handler)) return;

                if (subscriber.Handler.CanBeAwaited())
                {
                    await Task.Run(() => subscriber.Handler(command));
                }
                else
                {
                    subscriber.Handler(command);
                }

                OnAfterSendCommand(command, subscriber);
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

            foreach (var subscriber in subscribers)
            {
                OnBeforePublishEvent(@event, subscriber.Key, subscribers);

                var preparedEvent = PrepareEventForPublishing(@event, subscriber.Key, subscribers);

                if (!ShouldPublishEvent(preparedEvent, subscriber.Key, subscribers)) continue;

                Logger(string.Format("Publishing Event of type {0}, MessageId = {1}, Subscriber Key = {2}", typeof(TEvent).Name, @event.Id, subscriber.Key));

                if (subscriber.Handler.CanBeAwaited())
                {
                    var subscriber1 = subscriber;
                    await Task.Run(() => subscriber1.Handler(preparedEvent));
                }
                else
                {
                    subscriber.Handler(preparedEvent);
                }

                OnAfterPublishEvent(preparedEvent, subscriber.Key, subscribers);
            }
        }

        protected virtual void OnBeforeSendCommand(IMessage command, MessageSubscriber subscriber)
        {
            //no-op
        }

        protected virtual void OnAfterSendCommand(IMessage command, MessageSubscriber subscriber)
        {
            //no-op
        }

        protected virtual void OnBeforePublishEvent(IMessage @event, string subscriberKey, IList<MessageSubscriber> subscribers)
        {
            //no-op
        }

        protected virtual void OnAfterPublishEvent(IMessage @event, string subscriberKey, IList<MessageSubscriber> subscribers)
        {
            //no-op
        }

        protected virtual TCommand PrepareCommandForSending<TCommand>(TCommand command, MessageSubscriber subscriber) where TCommand : IMessage
        {
            command.Version = MessageVersion;
            return command;
        }

        protected virtual TEvent PrepareEventForPublishing<TEvent>(TEvent @event, string subscriberKey, IList<MessageSubscriber> subscribers) where TEvent : IMessage
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

            return new SubscribersResult(typeof(TMessage), hasSubscribers, subscribers);
        }

        protected virtual bool ShouldSendCommand(IMessage command, Action<IMessage> subscriber)
        {
            //effectively a no-op unless overridden in a derived class
            return true;
        }

        protected virtual bool ShouldPublishEvent(IMessage @event, string subscriberKey, IList<MessageSubscriber> subscribers)
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