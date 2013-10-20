namespace Proteus.Infrastructure.Messaging.Portable
{
    public class RetryPolicy
    {
        public int Retries { get; private set; }

        public RetryPolicy(int retries = 0)
        {
            Retries = retries;
        }
    }
}