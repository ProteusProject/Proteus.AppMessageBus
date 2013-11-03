using System;
using System.Collections.Generic;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable
    {
        private readonly List<Envelope<Event>> _queuedEvents = new List<Envelope<Event>>();
        private readonly List<Envelope<Command>> _queuedCommands = new List<Envelope<Command>>();

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

        public void Acknowledge<TMessage>(TMessage message) where TMessage : IMessage
        {
            if (message is Command)
            {
                var envelope = _queuedCommands.Single(env => env.Message.Id == message.Id);
                RecordAcknowledgement(envelope);
                
            }

            if (message is Event)
            {
                var envelope = _queuedEvents.Single(env => env.Message.Id == message.Id);
                RecordAcknowledgement(envelope);

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

        public void Publish<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : Event
        {
            StoreEvent(@event, retryPolicy);
            base.Publish(@event);
        }

        public override void Publish<TEvent>(TEvent @event)
        {
            Publish(@event, DefaultEventRetryPolicy);
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

        public void Send<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : Command
        {
            StoreCommand(command, retryPolicy);
            base.Send(command);
        }

        public override void Send<TCommand>(TCommand command)
        {
            Send(command, DefaultCommandRetryPolicy);
        }

        private void StoreCommand(Command command, RetryPolicy retryPolicy)
        {
            _queuedCommands.Add(new Envelope<Command>(command, retryPolicy));
        }
    }
}