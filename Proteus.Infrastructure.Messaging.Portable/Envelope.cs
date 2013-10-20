﻿using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class Envelope<TMessage> where TMessage : IMessage
    {
        private int _retriesRemaining;
        public TMessage Message { get; private set; }
        public RetryPolicy RetryPolicy { get; private set; }

        public bool ShouldRetry
        {
            get { return _retriesRemaining > 0; }
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
            _retriesRemaining--;
        }
    }
}