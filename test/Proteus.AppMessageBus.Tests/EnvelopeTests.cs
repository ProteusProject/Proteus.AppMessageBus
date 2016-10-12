#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using NUnit.Framework;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Tests
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