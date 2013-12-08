using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class DurableEvent : Event, IDurableEvent
    {
        public Guid AcknowledgementId { get; set; }
    }
}