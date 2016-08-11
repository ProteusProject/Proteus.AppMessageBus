using System;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public class DurableCommand : Command, IDurableCommand
    {
        public Guid AcknowledgementId { get; set; }
    }
}