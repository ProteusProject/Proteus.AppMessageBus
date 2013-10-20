using System;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class Envelope<TMessage> where TMessage : IMessage
    {
        public static Func<DateTime> DateTimeNowProvider { get; set; }

        private int _retriesRemaining;
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

        protected bool HasExpired
        {
            get { return RetryPolicy.Expiry.ToUniversalTime() - DateTimeNowProvider() <= TimeSpan.Zero; }
        }

        public Envelope(TMessage message)
            : this(message, new RetryPolicy())
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy)
        {
            if (null == DateTimeNowProvider)
            {
                DateTimeNowProvider = () => DateTime.UtcNow;
            }

            Message = message;
            RetryPolicy = retryPolicy;
            _retriesRemaining = retryPolicy.Retries;
        }

        public void HasBeenRetried()
        {
            _retriesRemaining--;
        }
    }
}