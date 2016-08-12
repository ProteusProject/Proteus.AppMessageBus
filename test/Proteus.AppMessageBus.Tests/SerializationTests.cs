using System;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using PCLStorage;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;
using Proteus.AppMessageBus.Portable.Serializable;

namespace Proteus.AppMessageBus.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void EnvelopesAreSerializable()
        {
            var retryPolicy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
            var msg = new DurableMessageBusTests.TestDurableEvent("0");
            var acknowledgmentId = Guid.NewGuid();

            var env = new Envelope<DurableMessageBusTests.TestDurableEvent>(msg, retryPolicy, acknowledgmentId);

            var serializer = new JsonNetSerializer();
            var serialized = serializer.SerializeToStream(env.EnvelopeState);
            var deserialized = serializer.Deserialize<EnvelopeState<DurableMessageBusTests.TestDurableEvent>>(serialized);

            var envelope = deserialized.GetEnvelope();
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope, Is.EqualTo(env));

            var message = deserialized.Message;
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo(msg.Payload));
            Assert.That(message.AcknowledgmentId, Is.EqualTo(msg.AcknowledgmentId));

        }

        [Test]
        public void TestMessageIsSerializable()
        {
            var msg = new DurableMessageBusTests.TestDurableEvent("0");

            var serializer = new JsonNetSerializer();

            var serialized = serializer.SerializeToStream(msg);
            var deserialized = serializer.Deserialize<DurableMessageBusTests.TestDurableEvent>(serialized);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.Payload, Is.EqualTo(msg.Payload));
            Assert.That(deserialized.AcknowledgmentId, Is.EqualTo(msg.AcknowledgmentId));
            Assert.That(deserialized.Version, Is.EqualTo(msg.Version));
        }

    }
}