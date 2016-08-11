using System;

namespace Proteus.Infrastructure.Messaging.Portable
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
    }
}