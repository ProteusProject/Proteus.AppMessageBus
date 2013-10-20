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

        public override void Publish<TEvent>(TEvent @event)
        {
            StoreEvent(@event);
            base.Publish(@event);
        }

        private void StoreEvent(Event @event)
        {
            _queuedEvents.Add(new Envelope<Event>(@event));
        }

        public override void Send<TCommand>(TCommand command)
        {
            StoreCommand(command);
            base.Send(command);
        }

        private void StoreCommand(Command command)
        {
            _queuedCommands.Add(new Envelope<Command>(command));
        }
    }

    public class Envelope<TMessage> where TMessage : IMessage
    {
        public TMessage Message { get;  private set; }

        public Envelope(TMessage message)
        {
            Message = message;
        }
    }
}