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
using Proteus.AppMessageBus.Serializable;

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