using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class CommandTx : Command, IMessageTx
    {
        public Guid AcknowledgementId { get; set; }
    }
}