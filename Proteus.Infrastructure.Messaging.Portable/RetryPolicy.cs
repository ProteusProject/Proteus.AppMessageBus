using System;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class RetryPolicy
    {
        public int Retries { get; private set; }
        public DateTime Expiry { get; private set; }
        public static Func<DateTime> DateTimeProvider { get; set; }

        public RetryPolicy()
            : this(0, TimeSpan.Zero)
        {
        }

        public RetryPolicy(int retries)
            : this(retries, TimeSpan.Zero)
        {
        }

        public RetryPolicy(int retries, TimeSpan expiryDuration)
        {
            if (null == DateTimeProvider)
            {
                DateTimeProvider = () => DateTime.UtcNow;
            }

            Expiry = DateTimeProvider() + expiryDuration;
            Retries = retries;
        }
    }
}