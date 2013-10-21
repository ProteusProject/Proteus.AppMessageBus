using System;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public class Command : IMessage
    {
        private readonly Guid _id = Guid.NewGuid();
        
        public Guid Id
        {
            get { return _id; }
        }
    }
}