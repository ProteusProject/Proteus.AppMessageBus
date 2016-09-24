﻿using System;
using Proteus.AppMessageBus.Portable.Abstractions;
using Proteus.AppMessageBus.Portable.Serializable;

namespace Proteus.AppMessageBus.Portable
{
    public class Envelope<TMessage> : IEquatable<Envelope<TMessage>> where TMessage : IDurableMessage
    {
        public string SubscriberKey { get; private set; }

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
            return Equals((Envelope<TMessage>)obj);
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

        private TMessage _message;

        public TMessage Message
        {
            get
            {
                _message.AcknowledgmentId = AcknowledgmentId;
                return _message;
            }
            private set { _message = value; }
        }

        public RetryPolicy RetryPolicy { get; private set; }

        public bool ShouldRetry
        {
            get { return HasRetriesRemaining && !HasExpired; }
        }

        public bool MessageMatchesVersion(string version)
        {
            return Message.Version == version;
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
            : this(message, new RetryPolicy(), Guid.NewGuid())
        {
        }

        public Envelope(TMessage message, RetryPolicy retryPolicy, Guid acknowledgmentId, string subscriberKey = "")
        {
            AcknowledgmentId = acknowledgmentId;
            SubscriberKey = subscriberKey;
            Message = message;
            RetryPolicy = retryPolicy;
            _retriesRemaining = retryPolicy.Retries;
        }

        public Envelope(EnvelopeState<TMessage> state)
        {
            SubscriberKey = state.SubscriberKey;
            _id = state.Id;
            _retriesRemaining = state.RetriesRemaining;
            _message = state.Message;
            RetryPolicy = state.RetryPolicyState.GetRetryPolicy();
            AcknowledgmentId = state.AcknowledgmentId;
        }

        public Guid AcknowledgmentId { get; private set; }

        public void HasBeenRetried()
        {
            _retriesRemaining = ZeroSafeDecrement(_retriesRemaining);
        }

        private int ZeroSafeDecrement(int value)
        {
            value = value - 1;
            return value < 0 ? 0 : value;
        }

        public EnvelopeState<TMessage> EnvelopeState
        {
            get
            {
                return new EnvelopeState<TMessage>()
                    {
                        AcknowledgmentId = AcknowledgmentId,
                        Id = Id,
                        Message = Message,
                        RetriesRemaining = _retriesRemaining,
                        RetryPolicyState = RetryPolicy.RetryPolicyState,
                        SubscriberKey = SubscriberKey
                    };
            }
        }

    }
}