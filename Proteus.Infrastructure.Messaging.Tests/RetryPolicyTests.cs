using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;

namespace Proteus.Infrastructure.Messaging.Tests
{
    [TestFixture]
    public class RetryPolicyTests
    {
        [Test]
        public void DefaultCtorSetsZeroRetries()
        {
            var policy = new RetryPolicy();
            Assert.That(policy.Retries, Is.EqualTo(0));
        }

        [Test]
        public void CanSetRetriesInCtor()
        {
            var policy = new RetryPolicy(10);
            Assert.That(policy.Retries, Is.EqualTo(10));

        }
    }
}