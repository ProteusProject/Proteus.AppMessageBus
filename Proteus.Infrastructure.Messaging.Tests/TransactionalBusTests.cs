using System;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;

namespace Proteus.Infrastructure.Messaging.Tests
{

    public class TransactionalBusTests
    {
        [TestFixture]
        public class WhenConfiguredWithNonZeroEventRetryAndCommandRetryAndMessagesHaveNotExpired
        {
            private TransactionalMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private readonly string _doubleValue = string.Format("{0}{0}", SingleValue);
            private const string SingleValue = "ImTheMessagePayload";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.Positive_OneHourTimeSpan());
                _bus = new TransactionalMessageBus(retryPolicy, retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEvent>(_events.Handle);
            }

            [Test]
            public void CommandAndEventAreRetriedOnNextStart()
            {
                _bus.Send(new TestCommand(SingleValue));
                _bus.Publish(new TestEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

            [Test]
            public void CommandAndEventRetriesRespectRetryPolicyAcrossAdditionalStarts()
            {
                _bus.Send(new TestCommand(SingleValue));
                _bus.Publish(new TestEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                //despite multiple calls to Start(), messages are only retried ONCE as per the retry policy setting
                for (int i = 0; i < 10; i++)
                {
                    _bus.Start();
                }

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

        }

        [TestFixture]
        public class WhenConfiguredWithZeroEventRetryAndZeroCommandRetry
        {
            private TransactionalMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private readonly string _doubleValue = string.Format("{0}{0}", SingleValue);
            private const string SingleValue = "ImTheMessagePayload";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy();
                Assume.That(retryPolicy.Retries, Is.EqualTo(0));

                _bus = new TransactionalMessageBus(retryPolicy, retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEvent>(_events.Handle);
            }

            [Test]
            public void CommandAndEventAreNotRetriedAcrossAdditionalStarts()
            {
                _bus.Send(new TestCommand(SingleValue));
                _bus.Publish(new TestEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                //despite multiple calls to Start(), messages are only retried ONCE as per the retry policy setting
                for (int i = 0; i < 10; i++)
                {
                    _bus.Start();
                }

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }
        }

        [TestFixture]
        public class WhenConfiguredWithNonZeroEventRetryAndNonZeroCommandRetryAndMessagesHaveAlreadyExpired
        {
            private TransactionalMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private const string SingleValue = "ImTheMessagePayload";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.Negative_OneHourTimeSpan());
                _bus = new TransactionalMessageBus(retryPolicy, retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEvent>(_events.Handle);
            }

            [Test]
            public void CommandAndEventAreNotRetriedOnNextStart()
            {
                _bus.Send(new TestCommand(SingleValue));
                _bus.Publish(new TestEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

        }
    }
}