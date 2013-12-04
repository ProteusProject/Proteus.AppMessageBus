using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Proteus.Infrastructure.Messaging.Portable.Serializable;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class DurableMessageBus : MessageBus, IStartableAsync, IStoppableAsync, ISendDurableCommands, IPublishDurableEvents, IAcceptMessageAcknowledgements
    {
        private List<Envelope<IDurableMessage>> _queuedEvents = new List<Envelope<IDurableMessage>>();
        private List<Envelope<IDurableMessage>> _queuedCommands = new List<Envelope<IDurableMessage>>();
        private RetryPolicy _activeRetryPolicy;
        private Lazy<string> _messageVersion = new Lazy<string>(() => string.Empty);

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }
        public IMessageSerializer Serializer { get; set; }
        public MesssagePersistence MessagePersister { get; set; }

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

        public DurableMessageBus()
            : this(new RetryPolicy(), new RetryPolicy())
        {
        }

        public DurableMessageBus(RetryPolicy defaultMessageRetryPolicy)
            : this(defaultMessageRetryPolicy, defaultMessageRetryPolicy)
        {
        }

        public DurableMessageBus(RetryPolicy defaultCommandRetryPolicy, RetryPolicy defaultEventRetryPolicy)
        {
            DefaultCommandRetryPolicy = defaultCommandRetryPolicy;
            DefaultEventRetryPolicy = defaultEventRetryPolicy;

            Serializer = new JsonNetSerializer();
            MessagePersister = new MesssagePersistence();
        }

        async public Task Start()
        {
            await LoadPendingMessages();

            ClearExpiredCommands();
            ProcessPendingCommands();

            ClearExpiredEvents();
            ProcessPendingEvents();
        }

        async public Task Stop()
        {
            await SavePendingMessages();
        }

        async private Task SavePendingMessages()
        {
            var queuedCommandStates = _queuedCommands.Select(cmdEnvelope => cmdEnvelope.EnvelopeState).ToList();
            var queuedEventStates = _queuedEvents.Select(evtEnvelope => evtEnvelope.EnvelopeState).ToList();

            var hasQueuedCommands = queuedCommandStates.Count > 0;
            var hasQueuedEvents = queuedEventStates.Count > 0;

            //process any commands or remove the stale data file if it exists
            if (hasQueuedCommands)
            {
                string commands;

                if (Serializer.TrySerializeToString(queuedCommandStates, out commands))
                {
                    await MessagePersister.SaveCommands(commands);
                }
            }
            else
            {
                await MessagePersister.RemoveAllCommandsFromPersistence();
            }

            //process any events or remove the stale data file if it exists
            if (hasQueuedEvents)
            {
                string events;

                if (Serializer.TrySerializeToString(queuedEventStates, out events))
                {
                    await MessagePersister.SaveEvents(events);
                }
            }
            else
            {
                await MessagePersister.RemoveAllEventsFromPersistence();
            }
        }


        async private Task LoadPendingMessages()
        {
            var hasNoQueuedCommands = _queuedCommands.Count == 0;
            var hasNoQueuedEvents = _queuedEvents.Count == 0;

            if (hasNoQueuedCommands)
            {
                if (await MessagePersister.CheckForCommands())
                {
                    var commands = await MessagePersister.LoadCommands();

                    List<EvenvelopeState<IDurableMessage>> queuedCommandStates;

                    if (Serializer.TryDeserialize(commands, out queuedCommandStates))
                    {
                        _queuedCommands = queuedCommandStates.Select(state => state.GetEnvelope()).ToList();
                    }
                }
            }

            if (hasNoQueuedEvents)
            {
                if (await MessagePersister.CheckForEvents())
                {
                    var events = await MessagePersister.LoadEvents();
                    
                    List<EvenvelopeState<IDurableMessage>> queuedEventStates;

                    if(Serializer.TryDeserialize<List<EvenvelopeState<IDurableMessage>>>(events, out queuedEventStates))
                    {
                        _queuedEvents = queuedEventStates.Select(state => state.GetEnvelope()).ToList();
                    }
                }
            }
        }

        private void ClearExpiredCommands()
        {
            _queuedCommands.RemoveAll(env => !env.ShouldRetry && !env.MessageMatchesVersion(MessageVersion));
        }

        private void ClearExpiredEvents()
        {
            _queuedEvents.RemoveAll(env => !env.ShouldRetry && !env.MessageMatchesVersion(MessageVersion));
        }

        private void ProcessPendingCommands()
        {
            foreach (var envelope in _queuedCommands.Where(envelope => envelope.ShouldRetry).ToList())
            {
                var subscribersResult = GetSubscribersFor(envelope.Message);

                //if there are no longer any subscribers to the message, we need to remove it from the queue
                //  so won't be around for further processing
                if (!subscribersResult.HasSubscribers || subscribersResult.Subscribers.Count < envelope.SubscriberIndex)
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

            Logger("Processing Pending Events...");

            var envelopes = _queuedEvents.Where(envelope => envelope.ShouldRetry).ToList();

            Logger(string.Format("{0} Pending Events found.", envelopes.Count));

            foreach (var envelope in envelopes)
            {
                var subscribersResult = GetSubscribersFor(envelope.Message);

                //if there are no longer any subscribers to the message, we need to remove it from the queue
                //  so won't be around for further processing
                if (!subscribersResult.HasSubscribers || subscribersResult.Subscribers.Count <= envelope.SubscriberIndex)
                {
                    Logger(string.Format("No Subscribers found for Envelope Id = {0}.  Removing from Pending Events.", envelope.Id));

                    _queuedEvents.Remove(envelope);
                    continue;
                }

                Logger(string.Format("Republishing Pending Event Id = {0} from Envelope Id = {1} to Subscriber Index = {2}", envelope.Message.Id, envelope.Id, envelope.SubscriberIndex));

                var subscriber = subscribersResult.Subscribers[envelope.SubscriberIndex];
                subscriber(envelope.Message);

                envelope.HasBeenRetried();

                if (!envelope.ShouldRetry)
                {
                    Logger(string.Format("Event in Envelope Id = {0} has invalid/expired Retry Policy.  Removing from Pending Events.", envelope.Id));
                    _queuedEvents.Remove(envelope);
                }
            }
        }

        public void Acknowledge<TMessage>(TMessage message) where TMessage : IDurableMessage
        {
            Logger(string.Format("Acknowledgement received for Message Id = {0} having Acknowledgement Id = {1}", message.Id,
                                 message.AcknowledgementId));

            if (message is ICommand)
            {
                Logger(string.Format("Acknowledging Command Id = {0} having Acknowledgement Id = {1}",
                                     message.Id, message.AcknowledgementId));

                _queuedCommands.RemoveAll(env => env.Message.AcknowledgementId == message.AcknowledgementId);
            }

            if (message is IEvent)
            {
                Logger(string.Format("Acknowledging Event Id = {0} having Acknowledgement Id = {1}",
                                     message.Id, message.AcknowledgementId));

                var acknowledgementId = message.AcknowledgementId;
                _queuedEvents.RemoveAll(env => env.Message.AcknowledgementId == acknowledgementId);
            }
        }


        protected override TCommand PrepareCommandForSending<TCommand>(TCommand command, Action<IMessage> subscribers)
        {
            Logger(string.Format("Preparing to Send Command of type {0}, MessageId = {1}", typeof(TCommand).AssemblyQualifiedName, command.Id));

            var durableCommand = command as IDurableMessage;

            if (null == durableCommand)
                return command;

            durableCommand.AcknowledgementId = Guid.NewGuid();
            durableCommand.Version = MessageVersion;

            StoreCommand(durableCommand);

            return (TCommand)durableCommand;
        }

        protected override TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            Logger(string.Format("Preparing to Publish Event of type {0}, MessageId = {1}, Subscriber Index = {2}", typeof(TEvent).AssemblyQualifiedName, @event.Id, subscriberIndex));

            var durableEvent = @event as IDurableMessage;

            if (null == durableEvent)
                return @event;

            durableEvent.AcknowledgementId = Guid.NewGuid();
            durableEvent.Version = MessageVersion;

            var clonedEvent = Clone((TEvent)durableEvent);

            StoreEvent((IDurableMessage)clonedEvent, subscriberIndex);

            return clonedEvent;
        }


        private TSource Clone<TSource>(TSource source)
        {
            var serialized = Serializer.SerializeToStream(source);
            return Serializer.Deserialize<TSource>(serialized);
        }

        public void SendDurable<TCommand>(TCommand command) where TCommand : ICommand, IDurableMessage
        {
            SendDurable(command, DefaultCommandRetryPolicy);
        }

        public void SendDurable<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IDurableMessage
        {
            Logger(string.Format("Sending Durable Command, Id = {0}", command.Id));

            _activeRetryPolicy = retryPolicy;
            base.Send(command);
        }

        public void PublishDurable<TEvent>(TEvent @event) where TEvent : IDurableMessage
        {
            PublishDurable(@event, DefaultEventRetryPolicy);
        }

        public void PublishDurable<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IDurableMessage
        {
            Logger(string.Format("Publishing Durable Event, Id = {0}", @event.Id));

            _activeRetryPolicy = retryPolicy;
            base.Publish(@event);
        }

        private void StoreEvent(IDurableMessage @event, int index)
        {
            var envelope = new Envelope<IDurableMessage>(@event, _activeRetryPolicy, @event.AcknowledgementId, index);

            if (envelope.ShouldRetry)
            {
                _queuedEvents.Add(envelope);
            }
        }

        private void StoreCommand(IDurableMessage command)
        {
            var envelope = new Envelope<IDurableMessage>(command, _activeRetryPolicy, command.AcknowledgementId);

            if (envelope.ShouldRetry)
            {
                _queuedCommands.Add(envelope);
            }

        }
    }
}