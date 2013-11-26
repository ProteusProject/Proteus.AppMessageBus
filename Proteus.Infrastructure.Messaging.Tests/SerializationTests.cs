using System;
using NUnit.Framework;
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
            var env = new Envelope<TransactionalBusTests.TestEventTx>(new TransactionalBusTests.TestEventTx("0"), retryPolicy, Guid.NewGuid());

            var serializer = new JsonNetSerializer();
            var serialized = serializer.SerializeToStream(env.EnvelopeState);
            var deserialized = serializer.Deserialize<EvenvelopeState<TransactionalBusTests.TestEventTx>>(serialized);

            var envelope = deserialized.GetEnvelope();

            Assert.That(deserialized.Message, Is.Not.Null);
        }

        [Test]
        public void TestMessageIsSerializable()
        {
            var msg = new TransactionalBusTests.TestEventTx("0");

            var serializer = new JsonNetSerializer();

            var serialized = serializer.SerializeToStream(msg);
            var deserialized = serializer.Deserialize<TransactionalBusTests.TestEventTx>(serialized);

            Assert.That(deserialized, Is.Not.Null);
        }

        [Test]
        public void AltMessageTest()
        {
            var policy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
            var msg = new TransactionalBusTests.TestEventTx("0");
            var env = new AltEnvelope<TransactionalBusTests.TestEventTx>();

            env.Message = msg;
            env.RetryPolicy = policy;

            var serializer = new JsonNetSerializer();

            var serialized = serializer.SerializeToStream(env);
            var deserialized = serializer.Deserialize<AltEnvelope<TransactionalBusTests.TestEventTx>>(serialized);

            Assert.That(deserialized.Message, Is.Not.Null);
            Assert.That(deserialized.RetryPolicy, Is.Not.Null);
            
        }

    }

    public class AltEnvelope<TMessage> where TMessage : IMessageTx
    {
        public RetryPolicy RetryPolicy { get; set; }
        public TMessage Message { get; set; }
    }
}