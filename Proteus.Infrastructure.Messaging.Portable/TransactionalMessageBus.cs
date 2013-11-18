using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, IStoppable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private readonly List<Envelope<IMessageTx>> _queuedEvents = new List<Envelope<IMessageTx>>();
        private readonly List<Envelope<IMessageTx>> _queuedCommands = new List<Envelope<IMessageTx>>();
        private RetryPolicy _activeRetryPolicy;

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }
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

        public bool Start()
        {
            ClearExpiredCommands();
            ProcessPendingCommands();

            ClearExpiredEvents();
            ProcessPendingEvents();

            return true;
        }

        public bool Stop()
        {
            return true;
        }

        private void ClearExpiredCommands()
        {
            _queuedCommands.RemoveAll(env => !env.ShouldRetry);
        }

        private void ClearExpiredEvents()
        {
            _queuedEvents.RemoveAll(env => !env.ShouldRetry);
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

        public void Acknowledge<TMessage>(TMessage message) where TMessage : IMessageTx
        {
            if (message is ICommand)
            {
                _queuedCommands.RemoveAll(env => env.Message.AcknowledgementId == message.AcknowledgementId);
            }

            if (message is IEvent)
            {
                var acknowledgementId = message.AcknowledgementId;
                _queuedEvents.RemoveAll(env => env.Message.AcknowledgementId == acknowledgementId);
            }
        }

        protected override TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            Logger(string.Format("Preparing to Publish Event of type {0}, MessageId = {1}, Subscriber Index = {2}", typeof(TEvent).AssemblyQualifiedName, @event.Id, subscriberIndex));

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

        public void SendTx<TCommand>(TCommand command) where TCommand : ICommand, IMessageTx
        {
            SendTx(command, DefaultCommandRetryPolicy);
        }

        public void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IMessageTx
        {
            base.Send(command);
            StoreCommand(command, retryPolicy);
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : IMessageTx
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx
        {
            _activeRetryPolicy = retryPolicy;
            base.Publish(@event);
        }

        protected override void OnAfterPublishEvent(IMessage @event, int index, List<Action<IMessage>> subscribers)
        {
            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return;

            var envelope = new Envelope<IMessageTx>(txEvent, _activeRetryPolicy, txEvent.AcknowledgementId, index);
            _queuedEvents.Add(envelope);
        }

        private void StoreCommand(IMessageTx command, RetryPolicy retryPolicy)
        {
            _queuedCommands.Add(new Envelope<IMessageTx>(command, retryPolicy, Guid.NewGuid()));
        }
    }
}