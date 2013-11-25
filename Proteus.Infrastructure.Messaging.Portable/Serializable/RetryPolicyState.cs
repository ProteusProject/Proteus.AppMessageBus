using System;

namespace Proteus.Infrastructure.Messaging.Portable.Serializable
{
    public class RetryPolicyState
    {
        public int Retries { get; set; }
        public DateTime Expiry { get; set; }
        
        public RetryPolicy GetRetryPolicy()
        {
            return new RetryPolicy(this);
        }
    }
}