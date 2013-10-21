using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IMessage
    {
        Guid Id { get; }
    }
}