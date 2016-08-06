using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{
    [TestFixture]
    public class MessageBusTests
    {
        private MessageBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new MessageBus() { Logger = text => Debug.WriteLine(text) };
        }

        [Test]
        public void CanCreateBus()
        {
            Assert.That(_bus, Is.Not.Null);
        }

        [Test]
        public async Task CanPreventMoreThanOneSubscriberRegisteredPerCommand()
        {
            var commands = new CommandSubscribers();
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);

            await AssertEx.ThrowsAsync<DuplicateSubscriberRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public async Task CanPreventNoSubscriberRegisteredForCommand()
        {
            await AssertEx.ThrowsAsync<NoSubscriberRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
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
        public async Task CanIgnoreNoSubscriberRegisteredForEvent()
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
        }


        [Test]
        public void MultipleSubscribersCanProcessMessageOnEventPublish()
        {
            const string input = "test";

            //two identical handlers will be registered for the single event,
            // so the result will be the input value twice
            var expected = string.Format("{0}{0}", input);

            var events = new EventSubscribers();
            _bus.RegisterSubscriptionFor<TestEvent>(events.Handle);
            _bus.RegisterSubscriptionFor<TestEvent>(events.Handle);

            _bus.Publish(new TestEvent(input));

            Assert.That(events.ProcessedMessagePayload, Is.EqualTo(expected));
        }

        [Test]
        public void SubscriberCanProcessMessageOnCommandSend()
        {
            const string expectedPayload = "payload";

            var commands = new CommandSubscribers();
            _bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);

            _bus.Send(new TestCommand(expectedPayload));

            Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(expectedPayload));
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
