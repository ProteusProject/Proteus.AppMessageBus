using System;
using System.Collections.Generic;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<Event>> _queuedEvents = new List<Envelope<Event>>();
        private readonly List<Envelope<Command>> _queuedCommands = new List<Envelope<Command>>();

        protected readonly Dictionary<Type, List<Action<Envelope<IMessage>>>> RoutesTx = new Dictionary<Type, List<Action<Envelope<IMessage>>>>();

        public void RegisterSubscriptionForTx<TMessage>(Action<Envelope<TMessage>> handler) where TMessage : IMessage 
        {
            List<Action<Envelope<IMessage>>> subscribers;
            if (!RoutesTx.TryGetValue(typeof(Envelope<TMessage>), out subscribers))
            {
                subscribers = new List<Action<Envelope<IMessage>>>();
                RoutesTx.Add(typeof(Envelope<TMessage>), subscribers);
            }
            subscribers.Add(DelegateConverter.CastArgument<Envelope<IMessage>, Envelope<TMessage>>(x => handler(x)));
        }

        //public void RegisterSubscriptionForTx<TEnvelope>(Action<TEnvelope> handler) where TEnvelope : Envelope<IMessage>
        //{
        //    List<Action<Envelope<IMessage>>> subscribers;
        //    if (!RoutesTx.TryGetValue(typeof(TEnvelope), out subscribers))
        //    {
        //        subscribers = new List<Action<Envelope<IMessage>>>();
        //        RoutesTx.Add(typeof(TEnvelope), subscribers);
        //    }
        //    subscribers.Add(DelegateConverter.CastArgument<Envelope<IMessage>, TEnvelope>(x => handler(x)));
        //}


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

        public void Acknowledge<TEnvelope>(TEnvelope envelope) where TEnvelope : Envelope<IMessage>
        {
            var message = envelope.Message;

            if (message is Command)
            {
                //RecordAcknowledgement(envelope);

            }

            if (message is Event)
            {
                //var envelope = _queuedEvents.Single(env => env.Message.Id == message.Id);
                //RecordAcknowledgement(envelope);

            }
        }

        private void RecordAcknowledgement(Envelope<Command> envelope)
        {
            throw new NotImplementedException();
        }

        private void RecordAcknowledgement(Envelope<Event> envelope)
        {
            throw new NotImplementedException();
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : Event
        {
            StoreEvent(@event, retryPolicy);
            PublishTransactionalEvent(@event);
        }

        private void PublishTransactionalEvent<TEvent>(TEvent @event) where TEvent : Event
        {
            List<Action<Envelope<IMessage>>> subscribers;
            if (!RoutesTx.TryGetValue(@event.GetType(), out subscribers)) return;
            foreach (var subscriber in subscribers)
            {
                //assign to local var to avoid the .net foreach bug
                var subscriberDelegate = subscriber;
                var envelope = new Envelope<IMessage>(@event);

                subscriberDelegate(envelope);
            }
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : Event
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }

        private void StoreEvent(Event @event, RetryPolicy retryPolicy)
        {
            _queuedEvents.Add(new Envelope<Event>(@event, retryPolicy));
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

        public void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : Command
        {
            StoreCommand(command, retryPolicy);
            base.Send(command);
        }

        public void SendTx<TCommand>(TCommand command) where TCommand : Command
        {
            SendTx(command, DefaultCommandRetryPolicy);
        }

        private void StoreCommand(Command command, RetryPolicy retryPolicy)
        {
            _queuedCommands.Add(new Envelope<Command>(command, retryPolicy));
        }
    }
}