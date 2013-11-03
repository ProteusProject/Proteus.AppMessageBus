using System;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class Envelope<TMessage> : IEquatable<Envelope<TMessage>> where TMessage : IMessage
    {
        public bool Equals(Envelope<TMessage> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Envelope<TMessage>) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(Envelope<TMessage> left, Envelope<TMessage> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Envelope<TMessage> left, Envelope<TMessage> right)
        {
            return !Equals(left, right);
        }

        private readonly Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get { return _id; }
        }

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

        private bool HasExpired
        {
            get { return RetryPolicy.Expiry.ToUniversalTime() - DateTime.UtcNow <= TimeSpan.Zero; }
        }

        public Envelope(TMessage message)
            : this(message, new RetryPolicy())
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy)
        {
            Message = message;
            RetryPolicy = retryPolicy;
            _retriesRemaining = retryPolicy.Retries;
        }

        public void HasBeenRetried()
        {
           _retriesRemaining = ZeroSafeDecrement(_retriesRemaining);
        }
        
        private int ZeroSafeDecrement(int value)
        {
            value = value -1;
            return value < 0 ? 0 : value;
        }

    }
}