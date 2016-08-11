using System;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IMessage
    {
        Guid Id { get; }
        DateTime UtcTimeStamp { get; }
        string Version { get; set; }
    }
}