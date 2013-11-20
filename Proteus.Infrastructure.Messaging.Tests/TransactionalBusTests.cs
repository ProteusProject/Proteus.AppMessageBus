using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

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
            private readonly string _doubleValue = String.Format("{0}{0}", SingleValue);
            private const string SingleValue = "0";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.Positive_OneHourTimeSpan());
                _bus = new TransactionalMessageBus(retryPolicy, retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommandTx>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_events.Handle);

            }

            [Test]
            public void CommandAndEventAreRetriedOnNextStart()
            {
                _bus.SendTx(new TestCommandTx(SingleValue));
                _bus.PublishTx(new TestEventTx(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

            [Test]
            public void CommandAndEventRetriesRespectRetryPolicyAcrossAdditionalStarts()
            {
                _bus.SendTx(new TestCommandTx(SingleValue));
                _bus.PublishTx(new TestEventTx(SingleValue));

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
            private readonly string _doubleValue = String.Format("{0}{0}", SingleValue);
            private const string SingleValue = "0";

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
            private const string SingleValue = "0";

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

        [TestFixture]
        public class Stuff
        {
            [Test]
            public void Test()
            {
                var retryPolicy = new RetryPolicy(20, DateTimeUtility.Positive_OneHourTimeSpan());
                var bus = new TransactionalMessageBus(retryPolicy);

                bus.Logger = (messge) => Debug.WriteLine("{0} - {1}", DateTime.Now, messge);

                var commands = new TransactionalCommandSubscribers();
                var events = new TransactionalEventSubscribers();
                var nonAckEvents = new TransactionalEventSubscriberThatIsNeverAcknowledged();
                bus.RegisterSubscriptionFor<TestCommandTx>(commands.Handle);
                bus.RegisterSubscriptionFor<TestEventTx>(events.Handle);

                bus.RegisterSubscriptionFor<TestEventTx>(nonAckEvents.Handle);

                const string singleResult = "0";
                var doubleResult = String.Format("{0}{0}", singleResult);

                var testCommand = new TestCommandTx(singleResult);
                bus.SendTx(testCommand);
                var testEvent = new TestEventTx(singleResult);
                bus.PublishTx(testEvent);

                //initial results of subscribers should be 1 for the command and 2 for the event
                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(singleResult));
                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(singleResult));

                bus.Start();

                //starting the bus should result in +1 (now 2x) to each of the payloads
                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(doubleResult));
                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(doubleResult));

                commands.AcknowledgeLastMessage(bus);
                events.AcknowledgeLastMessage(bus);

                for (int i = 0; i < 10; i++)
                {
                    bus.Start();
                }

                //now that all messages have been acknowledged, we should have the SAME results
                // even after the repeeated calls to bus.Start()
                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(doubleResult), "Commands not acknowledged properly!");
                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(doubleResult), "Events not acknowledged properly!");
                
                //unacknowledged events should be repeated ea time .Start() is called...
                Assert.That(nonAckEvents.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", singleResult)), "Unacknowledged Event not handled properly!");
            }
        }

        public class TransactionalEventSubscribers : TransactionalSubscribers, IHandleTransactional<TestEventTx>
        {
            public string ProcessedMessagePayload { get; private set; }
            public IList<Tuple<Guid, Guid>> ProcessedMessageIds { get; private set; }

            public void Handle(TestEventTx message)
            {
                ProcessedMessagePayload += message.Payload;
                ProcessedMessageIds.Add(new Tuple<Guid, Guid>(message.Id, message.AcknowledgementId));
                Messages.Add(message);
            }

            public TransactionalEventSubscribers()
            {
                ProcessedMessageIds = new List<Tuple<Guid, Guid>>();
            }
        }

        public class TransactionalEventSubscriberThatIsNeverAcknowledged : IHandleTransactional<TestEventTx>
        {
            public string ProcessedMessagePayload { get; private set; }
            public IList<Tuple<Guid, Guid>> ProcessedMessageIds { get; private set; }


            public void Handle(TestEventTx message)
            {
                ProcessedMessagePayload += message.Payload;
                ProcessedMessageIds.Add(new Tuple<Guid, Guid>(message.Id, message.AcknowledgementId));
            }

            public TransactionalEventSubscriberThatIsNeverAcknowledged()
            {
                ProcessedMessageIds = new List<Tuple<Guid, Guid>>();
            }
        }


        public class TransactionalSubscribers
        {
            public void AcknowledgeLastMessage(TransactionalMessageBus bus)
            {
                bus.Acknowledge(Messages.Last());
            }

            public IList<IMessageTx> Messages { get; private set; }

            public TransactionalSubscribers()
            {
                Messages = new List<IMessageTx>();
            }
        }

        public class TransactionalCommandSubscribers : TransactionalSubscribers, IHandleTransactional<TestCommandTx>
        {
            public string ProcessedMessagePayload { get; private set; }
            public IList<Tuple<Guid, Guid>> ProcessedMessageIds { get; private set; }

            public void Handle(TestCommandTx message)
            {
                ProcessedMessagePayload += message.Payload;
                ProcessedMessageIds.Add(new Tuple<Guid, Guid>(message.Id, message.AcknowledgementId));
                Messages.Add(message);
            }

            public TransactionalCommandSubscribers()
            {
                ProcessedMessageIds = new List<Tuple<Guid, Guid>>();
            }
        }

        public class TestEventTx : TestEvent, IMessageTx
        {
            public TestEventTx(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }


        public class TestCommandTx : TestCommand, IMessageTx
        {
            public TestCommandTx(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }

    }
}
