using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class EventTx : Event, IMessageTx
    {
        public Guid AcknowledgementId { get; set; }
    }
}