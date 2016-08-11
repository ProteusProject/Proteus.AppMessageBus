using System;

namespace Proteus.AppMessageBus.Portable.Serializable
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