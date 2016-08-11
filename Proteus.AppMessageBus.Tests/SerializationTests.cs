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
            var msg = new DurableMessageBusTests.TestDurableEvent("0");
            var acknowledgementId = Guid.NewGuid();

            var env = new Envelope<DurableMessageBusTests.TestDurableEvent>(msg, retryPolicy, acknowledgementId);

            var serializer = new JsonNetSerializer();
            var serialized = serializer.SerializeToStream(env.EnvelopeState);
            var deserialized = serializer.Deserialize<EvenvelopeState<DurableMessageBusTests.TestDurableEvent>>(serialized);

            var envelope = deserialized.GetEnvelope();
            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope, Is.EqualTo(env));

            var message = deserialized.Message;
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Id, Is.EqualTo(msg.Id));
            Assert.That(message.Payload, Is.EqualTo(msg.Payload));
            Assert.That(message.AcknowledgementId, Is.EqualTo(msg.AcknowledgementId));


        }

        [Test]
        public void TestMessageIsSerializable()
        {
            var msg = new DurableMessageBusTests.TestDurableEvent("0");

            var serializer = new JsonNetSerializer();

            var serialized = serializer.SerializeToStream(msg);
            var deserialized = serializer.Deserialize<DurableMessageBusTests.TestDurableEvent>(serialized);

            Assert.That(deserialized, Is.Not.Null);
        }

    }
}