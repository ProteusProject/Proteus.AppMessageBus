using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IDurableMessage : IMessage
    {
        Guid AcknowledgementId { get; set; }
    }
}