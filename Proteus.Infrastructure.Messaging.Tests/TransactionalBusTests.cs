using System;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;

namespace Proteus.Infrastructure.Messaging.Tests
{
    [TestFixture]
    public class TransactionalBusTests
    {
        [Test]
        public void UnhandledMessagesAreRetransmittedWhenBusIsRestarted()
        {
            const string singleValue = "value";
            var doubleValue = string.Format("{0}{0}", singleValue);

            var bus = new TransactionalMessageBus();
            var commands = new CommandSubscribers();
            var events = new EventSubscribers();

            bus.RegisterSubscriptionFor<TestCommand>(commands.Handle);
            bus.RegisterSubscriptionFor<TestEvent>(events.Handle);

            bus.Send(new TestCommand(singleValue));
            bus.Publish(new TestEvent(singleValue));

            Assume.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue));
            Assume.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue));

            bus.Start();

            Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(doubleValue));
            Assert.That(events.ProcessedMessagePayload, Is.EqualTo(doubleValue));
        }

    }
}