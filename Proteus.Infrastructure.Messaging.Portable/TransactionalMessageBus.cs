using System;
using System.Collections.Generic;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<IMessageTx>> _queuedEvents = new List<Envelope<IMessageTx>>();
        private readonly List<Envelope<IMessageTx>> _queuedCommands = new List<Envelope<IMessageTx>>();

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
                var envelopes = _queuedCommands.Where(env => env.Message.AcknowledgementId == message.AcknowledgementId);

                if (envelopes.Count() == 1)
                {
                    _queuedCommands.Remove(envelopes.First());
                }

            }

            //TODO: this wont work -- whoever first acknowledges the message will clear it under this model :(
            if (message is IEvent)
            {
                var envelopes = _queuedEvents.Where(env => env.Message.AcknowledgementId == message.AcknowledgementId);

                if (envelopes.Count() == 1)
                {
                    _queuedEvents.Remove(envelopes.First());
                }

            }
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx
        {
            StoreEvent(@event, retryPolicy);
            base.Publish(@event);
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : IMessageTx
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }

        private void StoreEvent(IMessageTx @event, RetryPolicy retryPolicy)
        {
            _queuedEvents.Add(new Envelope<IMessageTx>(@event, retryPolicy));
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

        private void StoreCommand(IMessageTx command, RetryPolicy retryPolicy)
        {
            _queuedCommands.Add(new Envelope<IMessageTx>(command, retryPolicy));
        }
    }
}