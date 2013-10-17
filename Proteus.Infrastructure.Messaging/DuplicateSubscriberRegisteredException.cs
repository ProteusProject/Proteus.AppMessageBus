using System;
using System.Runtime.Serialization;

namespace Proteus.Infrastructure.Messaging
{
    public class DuplicateSubscriberRegisteredException : InvalidOperationException
    {
        public DuplicateSubscriberRegisteredException()
        {
        }

        public DuplicateSubscriberRegisteredException(string message)
            : base(message)
        {
        }

        public DuplicateSubscriberRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DuplicateSubscriberRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}