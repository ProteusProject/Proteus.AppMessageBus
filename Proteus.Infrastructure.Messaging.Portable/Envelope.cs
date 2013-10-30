using System;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class Envelope<TMessage> where TMessage : IMessage
    {
        private int _retriesRemaining;
        private int _acknowedgementsRemaining;
        public TMessage Message { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }

        public bool ShouldRetry
        {
            get { return HasRetriesRemaining && !HasExpired; }
        }

        private bool HasRetriesRemaining
        {
            get { return _retriesRemaining > 0; }
        }

        private bool HasExpired
        {
            get { return RetryPolicy.Expiry.ToUniversalTime() - DateTime.UtcNow <= TimeSpan.Zero; }
        }

        public Envelope(TMessage message)
            : this(message, new RetryPolicy(), 1)
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy, int subscriberCount)
        {
            Message = message;
            RetryPolicy = retryPolicy;
            _retriesRemaining = retryPolicy.Retries;
            _acknowedgementsRemaining = subscriberCount;
        }

        public void HasBeenRetried()
        {
           _retriesRemaining = ZeroSafeDecrement(_retriesRemaining);
        }
        
        public void HasBeenAcknowledged()
        {
            _acknowedgementsRemaining = ZeroSafeDecrement(_acknowedgementsRemaining);
        }

        private int ZeroSafeDecrement(int value)
        {
            value = value -1;
            return value < 0 ? 0 : value;
        }

    }
}