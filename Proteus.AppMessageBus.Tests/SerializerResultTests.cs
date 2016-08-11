using System;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{

    public class SerializerResultTests
    {
        [TestFixture]
        public class WhenValueIsAvailable
        {
            private SerializerResult<string> _result;

            [SetUp]
            public void SetUp()
            {
                _result = new SerializerResult<string>(string.Empty, true);
            }

            [Test]
            public void ResultHasValue()
            {
                Assert.That(_result.Value, Is.Not.Null);
            }

            [Test]
            public void ResultDoesNotHaveException()
            {
                Assert.That(_result.Exception, Is.Null);
            }
        }

        [TestFixture]
        public class WhenValueIsNotAvailable
        {
            private SerializerResult<string> _result;

            [SetUp]
            public void SetUp()
            {
                _result = new SerializerResult<string>(null, false, new Exception());
            }

            [Test]
            public void ResultDoesNotHaveValue()
            {
                Assert.That(_result.HasValue, Is.False);
            }

            [Test]
            public void CannotAccessValueInResult()
            {
                Assert.Throws<InvalidOperationException>(() => { var attempt = _result.Value; });
            }

            [Test]
            public void ResultHasExpception()
            {
                Assert.That(_result.Exception, Is.Not.Null);
            }
        }
    }
}