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

using System.Runtime.Serialization;
using NUnit.Framework;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Tests
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