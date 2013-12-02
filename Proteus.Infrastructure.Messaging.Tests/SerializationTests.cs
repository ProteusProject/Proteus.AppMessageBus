using System;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Proteus.Infrastructure.Messaging.Portable.Serializable;

namespace Proteus.Infrastructure.Messaging.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void EnvelopesAreSerializable()
        {
            var retryPolicy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
            var env = new Envelope<DurableBusTests.TestDurableEvent>(new DurableBusTests.TestDurableEvent("0"),
                                                                      retryPolicy, Guid.NewGuid());

            var serializer = new JsonNetSerializer();
            var serialized = serializer.SerializeToStream(env.EnvelopeState);
            var deserialized = serializer.Deserialize<EvenvelopeState<DurableBusTests.TestDurableEvent>>(serialized);

            var envelope = deserialized.GetEnvelope();

            Assert.That(deserialized.Message, Is.Not.Null);
        }

        [Test]
        public void TestMessageIsSerializable()
        {
            var msg = new DurableBusTests.TestDurableEvent("0");

            var serializer = new JsonNetSerializer();

            var serialized = serializer.SerializeToStream(msg);
            var deserialized = serializer.Deserialize<DurableBusTests.TestDurableEvent>(serialized);

            Assert.That(deserialized, Is.Not.Null);
        }

    }
}