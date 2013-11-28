﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Proteus.Infrastructure.Messaging.Portable.Serializable;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable, IStoppable, ISendTransactionalCommands, IPublishTransactionalEvents, IAcceptMessageAcknowledgements
    {
        private List<Envelope<IMessageTx>> _queuedEvents = new List<Envelope<IMessageTx>>();
        private List<Envelope<IMessageTx>> _queuedCommands = new List<Envelope<IMessageTx>>();
        private RetryPolicy _activeRetryPolicy;
        private Lazy<string> _messageVersion = new Lazy<string>(() => string.Empty);

        protected RetryPolicy DefaultEventRetryPolicy { get; private set; }
        protected RetryPolicy DefaultCommandRetryPolicy { get; private set; }
        public ISerializer Serializer { get; set; }

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

        public TransactionalMessageBus()
            : this(new RetryPolicy(), new RetryPolicy())
        {
        }

        public TransactionalMessageBus(RetryPolicy defaultMessageRetryPolicy)
            : this(defaultMessageRetryPolicy, defaultMessageRetryPolicy)
        {
        }

        public TransactionalMessageBus(RetryPolicy defaultCommandRetryPolicy, RetryPolicy defaultEventRetryPolicy)
        {
            DefaultCommandRetryPolicy = defaultCommandRetryPolicy;
            DefaultEventRetryPolicy = defaultEventRetryPolicy;

            Serializer = new JsonNetSerializer();
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
            var queuedCommandStates = _queuedCommands.Select(command => command.EnvelopeState).ToList();
            var queuedEventStates = _queuedEvents.Select(@event => @event.EnvelopeState).ToList();

            var hasCommandsToProcess = queuedCommandStates.Count > 0;
            var hasEventsToProcess = queuedEventStates.Count > 0;

            IFolder folder = null;

            if (hasCommandsToProcess || hasEventsToProcess)
            {
                var rootFolder = FileSystem.Current.LocalStorage;
                folder = await rootFolder.CreateFolderAsync("Proteus.Messaging.Messages", CreationCollisionOption.OpenIfExists);
            }

            if (null==folder)
            {
                return;
            }

            if (hasCommandsToProcess)
            {
                var commands = Serializer.SerializeToString(queuedCommandStates);

                var commandsDatafile = await folder.CreateFileAsync("Commands.data", CreationCollisionOption.ReplaceExisting);
                await commandsDatafile.WriteAllTextAsync(commands);
            }

            if (hasEventsToProcess)
            {
                var events = Serializer.SerializeToString(queuedEventStates);
                var eventsDatafile = await folder.CreateFileAsync("Events.data", CreationCollisionOption.ReplaceExisting);
                await eventsDatafile.WriteAllTextAsync(events);
            }
        }


        async private Task LoadPendingMessages()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFolder folder = null;

            var folders = await rootFolder.GetFoldersAsync();

            foreach (var candidate in folders)
            {
                if (candidate.Name == "Proteus.Messaging.Messages")
                {
                    folder = candidate;
                }
            }

            if (null == folder)
            {
                return;
            }

            var hasNoQueuedCommands = _queuedCommands.Count == 0;
            var hasNoQueuedEvents = _queuedEvents.Count == 0;

            if (hasNoQueuedCommands)
            {
                IFile commandsDatafile = null;

                var files = await folder.GetFilesAsync();

                foreach (var file in files.Where(file => file.Name == "Commands.data"))
                {
                    commandsDatafile = file;
                }

                if (commandsDatafile != null)
                {
                    var commands = await commandsDatafile.ReadAllTextAsync();
                    var queuedCommandStates = Serializer.Deserialize<List<EvenvelopeState<IMessageTx>>>(commands);
                    _queuedCommands = queuedCommandStates.Select(state => state.GetEnvelope()).ToList();
                }
            }

            if (hasNoQueuedEvents)
            {
                IFile eventsDatafile = null;

                var files = await folder.GetFilesAsync();

                foreach (var file in files.Where(file => file.Name == "Events.data"))
                {
                    eventsDatafile = file;
                }

                if (eventsDatafile != null)
                {
                    var events = await eventsDatafile.ReadAllTextAsync();
                    var queuedEventStates = Serializer.Deserialize<List<EvenvelopeState<IMessageTx>>>(events);
                    _queuedEvents = queuedEventStates.Select(state => state.GetEnvelope()).ToList();
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

        public void Acknowledge<TMessage>(TMessage message) where TMessage : IMessageTx
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
            command.Version = MessageVersion;
            return command;
        }

        protected override TEvent PrepareEventForPublishing<TEvent>(TEvent @event, int subscriberIndex, List<Action<IMessage>> subscribers)
        {
            Logger(string.Format("Preparing to Publish Event of type {0}, MessageId = {1}, Subscriber Index = {2}", typeof(TEvent).AssemblyQualifiedName, @event.Id, subscriberIndex));

            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return @event;

            txEvent.AcknowledgementId = Guid.NewGuid();
            txEvent.Version = MessageVersion;

            return Clone((TEvent)txEvent);
        }


        private TSource Clone<TSource>(TSource source)
        {
            var serialized = Serializer.SerializeToStream(source);
            return Serializer.Deserialize<TSource>(serialized);
        }

        public void SendTx<TCommand>(TCommand command) where TCommand : ICommand, IMessageTx
        {
            SendTx(command, DefaultCommandRetryPolicy);
        }

        public void SendTx<TCommand>(TCommand command, RetryPolicy retryPolicy) where TCommand : ICommand, IMessageTx
        {
            Logger(string.Format("Transactionally sending Command Id = {0}", command.Id));

            base.Send(command);
            StoreCommand(command, retryPolicy);
        }

        public void PublishTx<TEvent>(TEvent @event) where TEvent : IMessageTx
        {
            PublishTx(@event, DefaultEventRetryPolicy);
        }

        public void PublishTx<TEvent>(TEvent @event, RetryPolicy retryPolicy) where TEvent : IMessageTx
        {
            Logger(string.Format("Transactionally publishing Event Id = {0}", @event.Id));

            _activeRetryPolicy = retryPolicy;
            base.Publish(@event);
        }

        protected override void OnAfterPublishEvent(IMessage @event, int index, List<Action<IMessage>> subscribers)
        {
            var txEvent = @event as IMessageTx;

            if (null == txEvent)
                return;

            StoreEvent(txEvent, index);
        }

        private void StoreEvent(IMessageTx @event, int index)
        {
            var envelope = new Envelope<IMessageTx>(@event, _activeRetryPolicy, @event.AcknowledgementId, index);

            if (envelope.ShouldRetry)
            {
                _queuedEvents.Add(envelope);
            }
        }

        private void StoreCommand(IMessageTx command, RetryPolicy retryPolicy)
        {
            var envelope = new Envelope<IMessageTx>(command, retryPolicy, Guid.NewGuid());

            if (envelope.ShouldRetry)
            {
                _queuedCommands.Add(envelope);
            }

        }
    }
}