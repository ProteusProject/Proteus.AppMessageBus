using System;
using System.Threading;
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
        public void DefaultCtorSetsZeroTimespanForExpiry()
        {
            var policy = new RetryPolicy();
            Assert.That(policy.Expiry - DateTime.UtcNow, Is.LessThanOrEqualTo(TimeSpan.Zero));
        }

        [Test]
        public void CanSetRetriesInCtor()
        {
            var policy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
            Assert.That(policy.Retries, Is.EqualTo(10));
        }
    }
}