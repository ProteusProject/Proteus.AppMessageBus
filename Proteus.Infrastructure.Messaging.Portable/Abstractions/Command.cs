using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class Command : IMessage
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly DateTime _utcTimeStamp = DateTime.UtcNow;

        public DateTime UtcTimeStamp
        {
            get { return _utcTimeStamp; }
        }

        public string Version { get; private set; }

        public Guid Id
        {
            get { return _id; }
        }
    }
}