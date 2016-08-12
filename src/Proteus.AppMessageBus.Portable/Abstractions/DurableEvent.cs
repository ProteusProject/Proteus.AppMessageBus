using System;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public class DurableEvent : Event, IDurableEvent
    {
        public Guid AcknowledgmentId { get; set; }
    }
}