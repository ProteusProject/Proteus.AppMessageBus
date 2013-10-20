using System;
using System.Collections.Generic;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class TransactionalMessageBus : MessageBus, IStartable
    {
        private readonly List<Event> _queuedEvents = new List<Event>();
        private readonly List<Command> _queuedCommands = new List<Command>();

        public void Start()
        {
            ProcessPendingCommands();
            ProcessPendingEvents();
        }

        private void ProcessPendingEvents()
        {
            foreach (var eventEntry in _queuedEvents)
            {
                base.Publish(eventEntry);
            }
        }

        private void ProcessPendingCommands()
        {
            foreach (var command in _queuedCommands)
            {
                base.Send(command);
            }
        }

        public override void Publish<TEvent>(TEvent @event)
        {
            StoreEvent(@event);
            base.Publish(@event);
        }

        private void StoreEvent(Event @event)
        {
            _queuedEvents.Add(@event);
        }

        public override void Send<TCommand>(TCommand command)
        {
            StoreCommand(command);
            base.Send(command);
        }

        private void StoreCommand(Command command)
        {
            _queuedCommands.Add(command);
        }
    }
}