using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class DurableEvent : Event, IDurableMessage
    {
        public Guid AcknowledgementId { get; set; }
    }
}