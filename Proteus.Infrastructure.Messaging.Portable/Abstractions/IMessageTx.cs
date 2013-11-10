using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IMessageTx : IMessage
    {
        Guid AcknowledgementId { get; set; }
    }
}