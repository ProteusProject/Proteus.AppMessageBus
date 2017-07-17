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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.AppMessageBus.Tests
{
    [TestFixture]
    public class AsyncDelegatesTests
    {
        private DelegateImplementer _implementer;
        private DelegateHolder _holder;

        [SetUp]
        public void SetUp()
        {
            _implementer = new DelegateImplementer();
            _holder = new DelegateHolder(_implementer.FuncThatReturnsString, _implementer.FuncThatReturnsTaskOfString, _implementer.FuncThatReturnsTask);
        }

        [Test]
        public async Task InvocationTest()
        {
            Assert.That(_holder.InvokeFuncThatReturnsString("steve"), Is.EqualTo("stevesteve"));
            Assert.That(await _holder.InvokeFuncThatReturnsTaskOfString("steve"), Is.EqualTo("stevesteve"));
        }

        [Test]
        public void CanValidateIfReturnTypeIsGenericTaskOfT()
        {
            var provider = _holder.FuncThatReturnsTaskOfString.Target;

            var asyncType = provider.GetType();
            var asyncMethodInfo = asyncType.GetMethod(_holder.FuncThatReturnsTaskOfString.Method.Name);
            var asyncReturnType = asyncMethodInfo.ReturnType;

            Assert.That(asyncReturnType.GetGenericTypeDefinition() == typeof(Task<>));

        }

        [Test]
        public void CanValidateIfReturnTypeIsTask()
        {
            var provider = _holder.FuncThatReturnsTask.Target;

            var asyncType = provider.GetType();
            var asyncMethodInfo = asyncType.GetMethod(_holder.FuncThatReturnsTask.Method.Name);
            var asyncReturnType = asyncMethodInfo.ReturnType;

            Assert.That(asyncReturnType == typeof(Task));
        }

        [Test]
        public void CanBeAwaitedCanProperlyValidateDelegates()
        {
            Assert.That(_holder.FuncThatReturnsString.CanBeAwaited(), Is.False);
            Assert.That(_holder.FuncThatReturnsTask.CanBeAwaited(), Is.True);
            Assert.That(_holder.FuncThatReturnsTaskOfString.CanBeAwaited(), Is.True);
        }

    }

    public class DelegateImplementer
    {
        public string FuncThatReturnsString(string input)
        {
            return input + input;
        }

        public async Task<string> FuncThatReturnsTaskOfString(string input)
        {
            return await Task.FromResult(FuncThatReturnsString(input));
        }

        public async Task FuncThatReturnsTask(string input)
        {
            await Task.FromResult(FuncThatReturnsString(input));
        }
    }

    public class DelegateHolder
    {
        public DelegateHolder(Func<string, string> funcThatReturnsString, Func<string, Task<string>> funcThatReturnsTaskOfString, Func<string, Task> funcThatReturnsTask)
        {
            FuncThatReturnsTaskOfString = funcThatReturnsTaskOfString;
            FuncThatReturnsTask = funcThatReturnsTask;
            FuncThatReturnsString = funcThatReturnsString;
        }

        public Func<string, string> FuncThatReturnsString { get; private set; }
        public Func<string, Task<string>> FuncThatReturnsTaskOfString { get; private set; }
        public Func<string, Task> FuncThatReturnsTask { get; private set; }

        public async Task<string> InvokeFuncThatReturnsTaskOfString(string input)
        {
            return await FuncThatReturnsTaskOfString(input);
        }

        public string InvokeFuncThatReturnsString(string input)
        {
            return FuncThatReturnsString(input);
        }

        public void InvokeFuncThatReturnsTask(string input)
        {
            FuncThatReturnsTask(input);
        }
    }
}