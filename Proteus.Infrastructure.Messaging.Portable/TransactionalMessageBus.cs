using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<IMessageTx>> _queuedEvents = new List<Envelope<IMessageTx>>();
        private readonly List<Envelope<IMessageTx>> _queuedCommands = new List<Envelope<IMessageTx>>();
        private RetryPolicy _activeRetryPolicy;

        public ISerializer Serializer { get; set; }

        public TransactionalMessageBus()
            : this(new RetryPolicy(), new RetryPolicy())
        {
        }

        public TransactionalMessageBus(RetryPolicy defaultCommandRetryPolicy, RetryPolicy defaultEventRetryPolicy)
        {
            DefaultCommandRetryPolicy = defaultCommandRetryPolicy;
            DefaultEventRetryPolicy = defaultEventRetryPolicy;

            Serializer = new JsonNetSerializer();
        }

        public void Start()
        {
            ClearExpiredCommands();
            ProcessPendingCommands();

            ClearExpiredEvents();
            ProcessPendingEvents();
        }

        private void ClearExpiredEvents()
        {
            _queuedEvents.RemoveAll(env => !env.ShouldRetry);
        }

        private void ClearExpiredCommands()
        {
            _queuedCommands.RemoveAll(env => !env.ShouldRetry);
        }

        private void ProcessPendingEvents()
        {
            var envelopes = _queuedEvents.Where(envelope => envelope.ShouldRetry).ToList();

            foreach (var envelope in envelopes)
            {
                var subscribersResult = GetSubscribersFor(envelope.Message);

                //if there are no longer any subscribers to the message, we need to remove it from the queue
                //  so won't be around for further processing
                if (!subscribersResult.HasSubscribers || subscribersResult.Subscribers.Count <= envelope.SubscriberIndex)
                {
                    _queuedEvents.Remove(envelope);
                    continue;
                }

                var subscriber = subscribersResult.Subscribers[envelope.SubscriberIndex];
                subscriber(envelope.Message);

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
                var subscribersResult = GetSubscribersFor(envelope.Message);

                //if there are no longer any subscribers to the message, we need to remove it from the queue
                //  so won't be around for further processing
                if (!subscribersResult.HasSubscribers || subscribersResult.Subscribers.Count <= envelope.SubscriberIndex)
                {
                    _queuedEvents.Remove(envelope);
                    continue;
                }

                var subscriber = subscribersResult.Subscribers[envelope.SubscriberIndex];
                subscriber(envelope.Message);

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
                Log(string.Format("Acknowledging command, message.Id={0}, AckId={1}", message.Id, message.AcknowledgementId));
                _queuedCommands.RemoveAll(env => env.Message.AcknowledgementId == message.AcknowledgementId);
            }

            if (message is IEvent)
            {
                Log(string.Format("Acknowledging event, message.Id={0}, AckId={1}", message.Id, message.AcknowledgementId));

                Log("QueuedEvents before removal...");
                foreach (var envelope in _queuedEvents)
                {
                    Log(string.Format("... EnvelopeId={0}, EnvelopeAckId={1}, EventId={2}, EventAckId={3}", envelope.Id, envelope.AcknowledgementId, envelope.Message.Id, envelope.Message.AcknowledgementId));
                }

                var acknowledgementId = message.AcknowledgementId;

                _queuedEvents.RemoveAll(env => env.Message.AcknowledgementId == acknowledgementId);

                Log("QueuedEvents after removal...");

                foreach (var envelope in _queuedEvents)
                {
                    Log(string.Format("... EnvelopeId={0}, EnvelopeAckId={1}, EventId={2}, EventAckId={3}", envelope.Id, envelope.AcknowledgementId, envelope.Message.Id, envelope.Message.AcknowledgementId));
                }
            }
        }

        protected override TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return @event;

            txEvent.AcknowledgementId = Guid.NewGuid();

            return Clone((TEvent)txEvent);
        }


        private TSource Clone<TSource>(TSource source)
        {
            var serialized = Serializer.Serialize(source);
            return Serializer.Deserialize<TSource>(serialized);
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx
        {
            _activeRetryPolicy = retryPolicy;
            base.Publish(@event);
        }

        protected override void OnAfterPublishEvent(IMessage @event, int index, List<Action<IMessage>> subscribers)
        {
#if LOG
            Log(string.Format("Preparing To post-publish process EventId={0}, AckId={1}", @event.Id,
                              ((IMessageTx)@event).AcknowledgementId));
#endif

            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return;

            var envelope = new Envelope<IMessageTx>(txEvent, _activeRetryPolicy, txEvent.AcknowledgementId, index);
            _queuedEvents.Add(envelope);

            Log(string.Format("Envelope stored with EnvelopeId={0}, EventId={1}, EnvelopeAckId={2}, EventAckId={3}", envelope.Id, txEvent.Id, envelope.AcknowledgementId, txEvent.AcknowledgementId));
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
            _queuedCommands.Add(new Envelope<IMessageTx>(command, retryPolicy, Guid.NewGuid()));
        }
    }
}