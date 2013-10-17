using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{
    [TestFixture]
    public class BusTests
    {
        private Bus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new Bus();
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
            _bus.RegisterSubscriberFor<TestCommand>(commands.Handle);
            _bus.RegisterSubscriberFor<TestCommand>(commands.Handle);

            Assert.Throws<DuplicateSubscriberRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanPreventNoSubscriberRegisteredForCommand()
        {
            Assert.Throws<NoSubscriberRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanReportNoSubscribers()
        {
            Assert.That(_bus.IsSubscriberRegisteredFor<TestCommand>(), Is.False);
        }

        [Test]
        public void CanReportSubscribers()
        {
            _bus.RegisterSubscriberFor<TestCommand>(new CommandSubscribers().Handle);
            Assert.That(_bus.IsSubscriberRegisteredFor<TestCommand>(), Is.True);
        }

        [Test]
        public void CanIgnoreNoSubscriberRegisteredForEvent()
        {
            //publish the event without registering any handlers
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(string.Empty)));
        }

        [Test]
        public void MultipleSubscribersCanProcessMessageOnEventPublish()
        {
            const string input = "test";

            //two identical handlers will be registered for the single event,
            // so the result will be the input value twice
            var expected = string.Format("{0}{0}", input);

            var events = new EventSubscribers();
            _bus.RegisterSubscriberFor<TestEvent>(events.Handle);
            _bus.RegisterSubscriberFor<TestEvent>(events.Handle);

            _bus.Publish(new TestEvent(input));

            Assert.That(events.ProcessedMessagePayload, Is.EqualTo(expected));
        }

        [Test]
        public void SubscriberCanProcessMessageOnCommandSend()
        {
            const string expectedPayload = "payload";

            var commands = new CommandSubscribers();
            _bus.RegisterSubscriberFor<TestCommand>(commands.Handle);

            _bus.Send(new TestCommand(expectedPayload));

            Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(expectedPayload));
        }

        public class TestEvent : Event
        {
            public string Payload { get; set; }

            public TestEvent(string payload)
            {
                Payload = payload;
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

        public class CommandSubscribers : IHandle<TestCommand>
        {
            public string ProcessedMessagePayload { get; private set; }

            public void Handle(TestCommand message)
            {
                ProcessedMessagePayload = message.Payload;
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
    }
}
