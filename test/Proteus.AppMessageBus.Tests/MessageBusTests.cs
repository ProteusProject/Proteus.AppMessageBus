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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using NUnit.Framework;
using Proteus.AppMessageBus.Abstractions;

namespace Proteus.AppMessageBus.Tests
{
    [TestFixture]
    public class MessageBusTests
    {
        private const string LogFileName = "log.txt";

        private MessageBus _bus;

        [SetUp]
        public void SetUp()
        {
            //since our tests for "successful" logging assert the log file exists,
            // we have to remove it before the tests run
            if (File.Exists(LogFileName))
            {
                File.Delete(LogFileName);
            }

            _bus = new MessageBus() { Logger = text => Debug.WriteLine(text) };
        }

        [Test]
        public void CanCreateBus()
        {
            Assert.That(_bus, Is.Not.Null);
        }

        [Test]
        public void CanPreventMoreThanOneSubscriberRegisteredPerCommand()
        {
            var commands = new CommandSubscribers();
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);

            Assert.ThrowsAsync<DuplicateSubscriberRegisteredException>(async () => await _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanPreventNoSubscriberRegisteredForCommand()
        {
            Assert.ThrowsAsync<NoSubscriberRegisteredException>(async () => await _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanReportNoSubscribers()
        {
            Assert.That(_bus.HasSubscriptionFor<TestCommand>(), Is.False);
        }

        [Test]
        public void CanReportSubscribers()
        {
            _bus.RegisterSubscriptionFor<TestCommand>(new CommandSubscribers().Handle);
            Assert.That(_bus.HasSubscriptionFor<TestCommand>(), Is.True);
        }

        [Test]
        public void CanIgnoreNoSubscriberRegisteredForEvent()
        {
            //publish the event without registering any handlers
            Assert.DoesNotThrowAsync(async () => await _bus.Publish(new TestEvent(string.Empty)));
        }

        [Test]
        public void CanClearSubscribers()
        {
            Assume.That(_bus.HasSubscriptionFor<TestCommand>(), Is.False, "Expected the bus to not have any subscribers for TestCommand");

            _bus.RegisterSubscriptionFor<TestCommand>(new CommandSubscribers().Handle);

            Assume.That(_bus.HasSubscriptionFor<TestCommand>(), Is.True, "Expected the bus to have a subscriber for TestCommand");

            _bus.RegisterSubscriptionFor<TestCommand>(new CommandSubscribers().Handle);
            _bus.UnRegisterAllSubscriptionsFor<TestCommand>();

            Assert.That(_bus.HasSubscriptionFor<TestCommand>(), Is.False, "Expected that all subscriptions for TestCommand would be cleared.");
        }


        [Test]
        public void CanUnregisterSubscriberByKey()
        {
            Assume.That(_bus.HasSubscriptionFor<TestEvent>(), Is.False, "Expected the bus to not have any subscriptions for TestEvent");

            var events = new EventSubscribers();
            _bus.RegisterSubscriptionFor<TestEvent>("Key1", events.Handle);
            _bus.RegisterSubscriptionFor<TestEvent>("Key2", events.Handle);

            Assume.That(_bus.HasSubscription("Key1"), Is.True, "Expected a subscriber registered with key: Key1");
            Assume.That(_bus.HasSubscription("Key2"), Is.True, "Expected a subscriber registered with key: Key2");

            _bus.UnRegisterSubscription("Key1");

            Assert.That(_bus.HasSubscription("Key1"), Is.False);
            Assert.That(_bus.HasSubscription("Key2"), Is.True);
        }


        [Test]
        public async Task MultipleSubscribersCanProcessMessageOnEventPublish()
        {
            const string input = "test";

            //two identical handlers will be registered for the single event,
            // so the result will be the input value twice
            var expected = string.Format("{0}{0}", input);

            var events = new EventSubscribers();
            _bus.RegisterSubscriptionFor<TestEvent>(events.Handle);
            _bus.RegisterSubscriptionFor<TestEvent>(events.Handle);

            await _bus.Publish(new TestEvent(input));

            Assert.That(events.ProcessedMessagePayload, Is.EqualTo(expected));
        }

        [Test]
        public async Task SubscriberCanProcessMessageOnCommandSend()
        {
            const string expectedPayload = "payload";

            var commands = new CommandSubscribers();
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);

            await _bus.Send(new TestCommand(expectedPayload));

            Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(expectedPayload));
        }


        [Test]
        public async Task CanLWireDelegateToLogToArbitraryLoggingFramework()
        {
            var busLogger = LogManager.GetLogger(this.GetType());
            var bus = new MessageBus() { Logger = text => busLogger.Debug(text) };

            bus.RegisterSubscriptionFor<TestCommand>(new CommandSubscribers().Handle);

            await bus.Send(new TestCommand("I am the logged payload"));

            Assume.That(File.Exists(LogFileName), "Unable to find log file; ensure logging is configured to write to expected filename.");

            var logFileEntries = File.ReadAllLines(LogFileName);

            Assert.That(logFileEntries.Any(entry => entry.Contains("Proteus.AppMessageBus.Tests.MessageBusTests Registering Subscriber for Messages of type TestCommand using Key ")));
            Assert.That(logFileEntries.Any(entry => entry.Contains("Proteus.AppMessageBus.Tests.MessageBusTests Sending Command of type TestCommand, MessageId = ")));

        }
    }

    public class EventSubscribers : IHandle<TestEvent>
    {
        public string ProcessedMessagePayload { get; private set; }

        public void Handle(TestEvent message)
        {
            ProcessedMessagePayload += message.Payload;
        }
    }

    public class CommandSubscribers : IHandle<TestCommand>
    {
        public string ProcessedMessagePayload { get; private set; }

        public void Handle(TestCommand message)
        {
            ProcessedMessagePayload += message.Payload;
        }
    }

    public class TestCommand : Command
    {
        public string Payload { get; set; }

        public TestCommand(string payload)
        {
            Payload = payload;
        }
    }

    public class TestEvent : Event
    {
        public string Payload { get; set; }

        public TestEvent(string payload)
        {
            Payload = payload;
        }
    }
}
