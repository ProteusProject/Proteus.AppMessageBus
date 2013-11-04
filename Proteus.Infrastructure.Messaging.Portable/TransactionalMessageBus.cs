using System;
using System.Collections.Generic;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<IEvent>> _queuedEvents = new List<Envelope<IEvent>>();
        private readonly List<Envelope<ICommand>> _queuedCommands = new List<Envelope<ICommand>>();

        protected readonly Dictionary<Type, List<Action<IMessageTx>>> RoutesTx = new Dictionary<Type, List<Action<IMessageTx>>>();

        public void RegisterSubscriptionForTx<TMessage>(Action<TMessage> handler) where TMessage : IMessageTx
        {
            List<Action<IMessageTx>> subscribers;
            if (!RoutesTx.TryGetValue(typeof(TMessage), out subscribers))
            {
                subscribers = new List<Action<IMessageTx>>();
                RoutesTx.Add(typeof(TMessage), subscribers);
            }
            subscribers.Add(DelegateConverter.CastArgument<IMessageTx, TMessage>(x => handler(x)));

            base.RegisterSubscriptionFor(handler);
        }

        public override bool HasSubscriptionFor<TMessage>()
        {
            return HasTransactionalSubscriptionFor<TMessage>() || base.HasSubscriptionFor<TMessage>();
        }

        private bool HasTransactionalSubscriptionFor<TMessage>()
        {
            return RoutesTx.ContainsKey(typeof(TMessage));
        }

        public override void UnRegisterAllSubscriptionsFor<TMessage>()
        {
            if (HasTransactionalSubscriptionFor<TMessage>())
            {
                RoutesTx.Remove(typeof(TMessage));
            }

            base.UnRegisterAllSubscriptionsFor<TMessage>();
        }


        public TransactionalMessageBus()
            : this(new RetryPolicy(), new RetryPolicy())
        {
        }

        public TransactionalMessageBus(RetryPolicy defaultCommandRetryPolicy, RetryPolicy defaultEventRetryPolicy)
        {
            DefaultCommandRetryPolicy = defaultCommandRetryPolicy;
            DefaultEventRetryPolicy = defaultEventRetryPolicy;
        }

        public void Start()
        {
            ProcessPendingCommands();
            ProcessPendingEvents();
        }

        private void ProcessPendingEvents()
        {
            foreach (var envelope in _queuedEvents.Where(envelope => envelope.ShouldRetry).ToList())
            {
                base.Publish(envelope.Message);
                envelope.HasBeenRetried();
                if (!envelope.ShouldRetry)
                {
                    _queuedEvents.Remove(envelope);
                }
            }
        }

        private void ProcessPendingCommands()
        {
            foreach (var envelope in _queuedCommands.Where(envelope => envelope.ShouldRetry).ToList())
            {
                base.Send(envelope.Message);
                envelope.HasBeenRetried();
                if (!envelope.ShouldRetry)
                {
                    _queuedCommands.Remove(envelope);
                }
            }
        }

        public void Acknowledge<TMessage>(TMessage message) where TMessage : IMessageTx
        {
            if (message is ICommand)
            {
                //RecordAcknowledgement(envelope);

            }

            if (message is IEvent)
            {
                //var envelope = _queuedEvents.Single(env => env.Message.Id == message.Id);
                //RecordAcknowledgement(envelope);

            }
        }

        private void RecordAcknowledgement(Envelope<ICommand> envelope)
        {
            throw new NotImplementedException();
        }

        private void RecordAcknowledgement(Envelope<IEvent> envelope)
        {
            throw new NotImplementedException();
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IEvent, IMessageTx
        {
            StoreEvent(@event, retryPolicy);
            PublishTransactionalEvent(@event);
        }

        private void PublishTransactionalEvent<TEvent>(TEvent @event) where TEvent : IEvent, IMessageTx
        {
            List<Action<IMessageTx>> subscribers;
            if (!RoutesTx.TryGetValue(@event.GetType(), out subscribers)) return;
            foreach (var subscriber in subscribers)
            {
                //assign to local var to avoid the .net foreach bug
                var subscriberDelegate = subscriber;
                subscriberDelegate(@event);
            }
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : IEvent, IMessageTx
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }

        private void StoreEvent(IEvent @event, RetryPolicy retryPolicy)
        {
            _queuedEvents.Add(new Envelope<IEvent>(@event, retryPolicy));
        }

        private int SubscriberCountFor(Type messageType)
        {
            List<Action<IMessage>> subscribers;

            if (Routes.TryGetValue(messageType, out subscribers))
            {
                return subscribers.Count;
            }

            return 0;
        }

        public void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IMessageTx
        {
            StoreCommand(command, retryPolicy);
            base.Send(command);
        }

        public void SendTx<TCommand>(TCommand command) where TCommand : ICommand, IMessageTx
        {
            SendTx(command, DefaultCommandRetryPolicy);
        }

        private void StoreCommand(ICommand command, RetryPolicy retryPolicy)
        {
            _queuedCommands.Add(new Envelope<ICommand>(command, retryPolicy));
        }
    }
}