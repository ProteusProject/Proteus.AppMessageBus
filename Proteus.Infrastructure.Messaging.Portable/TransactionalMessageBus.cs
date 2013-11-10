using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<IMessageTx>> _queuedEvents = new List<Envelope<IMessageTx>>();
        private readonly List<Envelope<IMessageTx>> _queuedCommands = new List<Envelope<IMessageTx>>();
        private RetryPolicy _activeRetryPolicy;
        private bool _isInStart;

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
            _isInStart = true;
            ProcessPendingCommands();
            ProcessPendingEvents();
            _isInStart = false;
        }

        private void ProcessPendingEvents()
        {
            var envelopes = _queuedEvents.Where(envelope => envelope.ShouldRetry).ToList();

            foreach (var envelope in envelopes)
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

                //Debug.Assert(envelopes.Count() <= 1);


                if (envelopes.Count() == 1)
                {
                    _queuedEvents.Remove(envelopes.First());
                }

            }
        }

        protected override TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            if (_isInStart)
            {
                return @event;
            }

            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return @event;

            //if (txEvent.AcknowledgementId == Guid.Empty)
            //{
            txEvent.AcknowledgementId = Guid.NewGuid();
            //}

            return (TEvent)txEvent;
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx
        {
            _activeRetryPolicy = retryPolicy;
            base.Publish(@event);
        }

        protected override bool ShouldPublishEvent(IMessage @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            var txEvent = @event as IMessageTx;

            if (!_isInStart || null == txEvent) return true;

            var shouldPublishEvent = _queuedEvents.Count(env => env.Message.Id == txEvent.Id && env.Message.AcknowledgementId == txEvent.AcknowledgementId && env.SubscriberIndex == subscriberIndex) == 1;
            return shouldPublishEvent;
        }

        protected override void OnAfterPublishEvent(IMessage @event, int index, List<Action<IMessage>> subscribers)
        {
            if (_isInStart)
                return;

            var txEvent = @event as IMessageTx;

            if (null != txEvent)
            {
                _queuedEvents.Add(new Envelope<IMessageTx>(txEvent, _activeRetryPolicy, index));
            }
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : IMessageTx
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }

        public void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IMessageTx
        {
            base.Send(command);
            StoreCommand(command, retryPolicy);
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