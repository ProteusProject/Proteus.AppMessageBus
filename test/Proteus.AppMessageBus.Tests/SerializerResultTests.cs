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
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Tests
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
            public void ResultHasException()
            {
                Assert.That(_result.Exception, Is.Not.Null);
            }
        }
    }
}