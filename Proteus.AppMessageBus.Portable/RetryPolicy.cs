using System;
using Proteus.AppMessageBus.Portable.Serializable;

namespace Proteus.AppMessageBus.Portable
{
    public class RetryPolicy
    {
        public int Retries { get; private set; }
        public DateTime Expiry { get; private set; }

        public RetryPolicyState RetryPolicyState
        {
            get
            {
                return new RetryPolicyState() { Retries = Retries, Expiry = Expiry };
            }
        }


        public RetryPolicy()
            : this(0, TimeSpan.Zero)
        {
        }

        public RetryPolicy(int retries, TimeSpan durationUntilExpiry)
        {
            Expiry = DateTime.UtcNow + durationUntilExpiry;
            Retries = retries;
        }

        public RetryPolicy(RetryPolicyState state)
        {
            Retries = state.Retries;
            Expiry = state.Expiry;
        }
    }
}