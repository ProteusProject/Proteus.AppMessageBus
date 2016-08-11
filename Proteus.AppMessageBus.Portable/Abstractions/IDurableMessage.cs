using System;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IDurableMessage : IMessage
    {
        Guid AcknowledgmentId { get; set; }
    }
}