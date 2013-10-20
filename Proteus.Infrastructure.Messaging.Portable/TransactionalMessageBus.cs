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
                envelope.Retried();
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
                envelope.Retried();
                if (!envelope.ShouldRetry)
                {
                    _queuedCommands.Remove(envelope);
                }
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

        public RetryPolicy(int retries = 0)
        {
            Retries = retries;
        }
    }

    public class Envelope<TMessage> where TMessage : IMessage
    {
        private int _retriesRemaining;
        public TMessage Message { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }

        public bool ShouldRetry
        {
            get { return _retriesRemaining > 0; }
        }

        public Envelope(TMessage message)
            : this(message, new RetryPolicy())
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy)
        {
            Message = message;
            RetryPolicy = retryPolicy;
            _retriesRemaining = retryPolicy.Retries;
        }

        public void Retried()
        {
            _retriesRemaining--;
        }
    }
}