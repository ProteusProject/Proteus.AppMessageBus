using System.Runtime.Serialization;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public class JsonNetSerializerTests
    {
        [TestFixture]
        public class WhenTryDeserializingEmptyString
        {
            private JsonNetSerializer _serializer;
            private readonly string _input = string.Empty;
            private SerializerResult<TestCommand> _result;

            [SetUp]
            public void SetUp()
            {
                _serializer = new JsonNetSerializer();
                _result = _serializer.TryDeserialize<TestCommand>(_input);
            }

            [Test]
            public void ReturnsNoValue()
            {
                Assert.That(_result.HasValue, Is.False);
                Assert.That(_result.Exception, Is.Not.Null);
            }

            [Test]
            public void ReturnsSerializationException()
            {
                Assert.That(_result.Exception, Is.Not.Null);
                Assert.That(_result.Exception, Is.InstanceOf<SerializationException>());
            }
        }

    }
}