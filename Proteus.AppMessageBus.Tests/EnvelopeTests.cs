using System;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public class EnvelopeTests
    {
        [TestFixture]
        public class WhenEnvelopeHasZeroRetryPolicyAndIsNotExpired
        {
            private RetryPolicy _retryPolicy;
            private Envelope<IDurableMessage> _envelope;

            [SetUp]
            public void SetUp()
            {
                _retryPolicy = new RetryPolicy(0, DateTimeUtility.PositiveOneHourTimeSpan);
                _envelope = new Envelope<IDurableMessage>(new DurableMessageBusTests.TestDurableCommand(string.Empty), _retryPolicy, Guid.NewGuid());
            }

            [Test]
            public void ShouldReportRetryIsNotNecessary()
            {
                Assume.That(_retryPolicy.Retries, Is.EqualTo(0));
                Assert.That(_envelope.ShouldRetry, Is.False);
            }

            [Test]
            public void RecordingAdditionalRetryDoesNotChangeShouldRetryState()
            {
                Assume.That(_envelope.ShouldRetry, Is.False);
                _envelope.HasBeenRetried();
                Assert.That(_envelope.ShouldRetry, Is.False);
            }

        }

        [TestFixture]
        public class WhenEnvelopeHasNonZeroRetryPolicyAndNotYetExpired
        {
            private RetryPolicy _retryPolicy;
            private Envelope<IDurableMessage> _envelope;

            [SetUp]
            public void SetUp()
            {
                _retryPolicy = new RetryPolicy(3, DateTimeUtility.PositiveOneHourTimeSpan);
                _envelope = new Envelope<IDurableMessage>(new DurableMessageBusTests.TestDurableCommand(string.Empty), _retryPolicy, Guid.NewGuid());
            }

            [Test]
            public void ShouldReportRetryIsNecessary()
            {
                Assert.That(_envelope.ShouldRetry, Is.True);
            }

            [Test]
            public void RecordingAdditionalRetryDoesNotChangeShouldRetryStateUntilRetriesAreExpended()
            {
                Assume.That(_retryPolicy.Retries, Is.EqualTo(3));
                Assume.That(_envelope.ShouldRetry, Is.True);

                //reduce retries to 2
                _envelope.HasBeenRetried();
                Assert.That(_envelope.ShouldRetry, Is.True);

                //reduce retries to 1
                _envelope.HasBeenRetried();
                Assert.That(_envelope.ShouldRetry, Is.True);

                //reduce retries to 0
                _envelope.HasBeenRetried();
                Assert.That(_envelope.ShouldRetry, Is.False);
            }
        }

        [TestFixture]
        public class WhenEnvelopeHasNoRetryPolicy
        {
            private Envelope<IDurableMessage> _envelope;

            [SetUp]
            public void SetUp()
            {
                _envelope = new Envelope<IDurableMessage>(new DurableMessageBusTests.TestDurableCommand(string.Empty));
            }

            [Test]
            public void UsesDefaultRetryPolicyOfZeroRetries()
            {
                Assert.That(_envelope.ShouldRetry, Is.False);
            }
        }
    }
}