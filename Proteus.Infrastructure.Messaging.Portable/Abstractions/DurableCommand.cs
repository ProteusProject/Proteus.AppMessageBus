using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class DurableCommand : Command, IDurableCommand
    {
        public Guid AcknowledgementId { get; set; }
    }
}