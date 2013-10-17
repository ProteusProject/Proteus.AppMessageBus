using System;
using System.Runtime.Serialization;

namespace Proteus.Infrastructure.Messaging
{
    public class NoSubscriberRegisteredException : InvalidOperationException
    {
        public NoSubscriberRegisteredException()
        {
        }

        public NoSubscriberRegisteredException(string message)
            : base(message)
        {
        }

        public NoSubscriberRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NoSubscriberRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}