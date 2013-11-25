using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class Message : IMessage
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly DateTime _utcTimeStamp = DateTime.UtcNow;

        public DateTime UtcTimeStamp
        {
            get { return _utcTimeStamp; }
        }

        public string Version { get; set; }

        public Guid Id
        {
            get { return _id; }
        }
    }
}