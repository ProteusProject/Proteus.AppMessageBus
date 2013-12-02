using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class DurableCommand : Command, IDurableMessage
    {
        public Guid AcknowledgementId { get; set; }
    }
}