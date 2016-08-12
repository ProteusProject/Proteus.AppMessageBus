using System;

namespace Proteus.AppMessageBus.Portable
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
    }
}