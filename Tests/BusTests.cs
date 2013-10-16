using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary1;
using ClassLibrary1.Abstractions;
using NUnit.Framework;

namespace Tests
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
        public void CanPreventMoreThanOneHandlerRegisteredPerCommand()
        {
            var commands = new CommandHandlers();
            _bus.RegisterHandlerFor<TestCommand>(commands.Handle);
            _bus.RegisterHandlerFor<TestCommand>(commands.Handle);

            Assert.Throws<DuplicateHandlerRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanPreventNoHandlerRegisteredForCommand()
        {
            Assert.Throws<NoHandlerRegisteredException>(() => _bus.Send(new TestCommand(string.Empty)));
        }

        [Test]
        public void CanIgnoreNoHandlerRegisteredForEvent()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(string.Empty)));
        }

        [Test]
        public void RegisteredHandlerCanProcessMessageOnEventPublish()
        {
            const string input = "test";

            //two identical handlers will be registered for the single event,
            // so the result will be the input value twice
            var expected = string.Format("{0}{0}", input);

            var events = new EventHandlers();
            _bus.RegisterHandlerFor<TestEvent>(events.Handle);
            _bus.RegisterHandlerFor<TestEvent>(events.Handle);

            _bus.Publish(new TestEvent(input));

            Assert.That(events.HandledMessagePayload, Is.EqualTo(expected));
        }

        public class EventHandlers : IHandle<TestEvent>
        {
            public string HandledMessagePayload { get; private set; }

            public void Handle(TestEvent message)
            {
                HandledMessagePayload += message.Payload;
            }
        }

        [Test]
        public void RegisteredHandlerCanProcessMessageOnCommandSend()
        {
            const string expectedPayload = "payload";

            var commands = new CommandHandlers();
            _bus.RegisterHandlerFor<TestCommand>(commands.Handle);

            _bus.Send(new TestCommand(expectedPayload));

            Assert.That(commands.HandledMessagePayload, Is.EqualTo(expectedPayload));
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

        public class CommandHandlers : IHandle<TestCommand>
        {
            public string HandledMessagePayload { get; private set; }

            public void Handle(TestCommand message)
            {
                HandledMessagePayload = message.Payload;
            }
        }
    }
}
