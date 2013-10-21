using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class Event : IMessage
    {
        public int Version;
        private readonly Guid _id = Guid.NewGuid();
        
        public Guid Id
        {
            get { return _id; }
        }
    }

}



