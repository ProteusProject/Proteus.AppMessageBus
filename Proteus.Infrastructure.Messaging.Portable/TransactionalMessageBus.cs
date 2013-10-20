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
        {
            DefaultCommandRetryPolicy = new RetryPolicy();
            DefaultEventRetryPolicy = new RetryPolicy();
        }

        public void Start()
        {
            ProcessPendingCommands();
            ProcessPendingEvents();
        }

        private void ProcessPendingEvents()
        {
            foreach (var eventEntry in _queuedEvents)
            {
                base.Publish(eventEntry.Message);
            }
        }

        private void ProcessPendingCommands()
        {
            foreach (var command in _queuedCommands)
            {
                base.Send(command.Message);
            }
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

    public class RetryPolicy
    {
        public int Retries { get; private set; }

        public RetryPolicy()
        {
            Retries = 0;
        }

        public RetryPolicy(int retries)
        {
            Retries = retries;
        }
    }

    public class Envelope<TMessage> where TMessage : IMessage
    {
        public TMessage Message { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }

        public Envelope(TMessage message)
            : this(message, new RetryPolicy())
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy)
        {
            Message = message;
            RetryPolicy = retryPolicy;
        }
    }
}