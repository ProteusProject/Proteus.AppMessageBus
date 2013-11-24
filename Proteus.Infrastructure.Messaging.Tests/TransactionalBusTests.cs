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
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new TransactionalMessageBus(retryPolicy);

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

                _bus = new TransactionalMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommandTx>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_events.Handle);
            }

            [Test]
            public void CommandAndEventAreNotRetriedAcrossAdditionalStarts()
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
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.NegativeOneHourTimeSpan);
                _bus = new TransactionalMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommandTx>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_events.Handle);
            }

            [Test]
            public void CommandAndEventAreNotRetriedOnNextStart()
            {
                _bus.SendTx(new TestCommandTx(SingleValue));
                _bus.PublishTx(new TestEventTx(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

        }


        [TestFixture]
        public class WhenAcknowledgingCommands
        {
            private TransactionalMessageBus _bus;
            private TransactionalCommandSubscribers _commands;
            private TransactionalEventSubscribers _events;
            private const string SingleValue = "0";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new TransactionalMessageBus(retryPolicy);

                _commands = new TransactionalCommandSubscribers();
                _events = new TransactionalEventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommandTx>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_events.Handle);
            }

            [Test]
            public void UnacknowledgedCommandWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var command = new TestCommandTx(SingleValue);
                _bus.SendTx(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            public void AcknowledgedCommandWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var command = new TestCommandTx(SingleValue);
                _bus.SendTx(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _commands.AcknowledgeLastMessage(_bus);
                _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }
        }

        [TestFixture]
        public class WhenAcknowledgingEvents
        {
            private TransactionalMessageBus _bus;
            private CommandSubscribers _commands;
            private TransactionalEventSubscribers _eventsThatWillBeAcknowledged;
            private TransactionalEventSubscribers _eventsThatWillNotBeAcknowledged;
            private const string SingleValue = "0";

            [SetUp]
            public void SetUp()
            {
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new TransactionalMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _eventsThatWillBeAcknowledged = new TransactionalEventSubscribers();
                _eventsThatWillNotBeAcknowledged = new TransactionalEventSubscribers();
                _bus.RegisterSubscriptionFor<TestCommandTx>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_eventsThatWillBeAcknowledged.Handle);
                _bus.RegisterSubscriptionFor<TestEventTx>(_eventsThatWillNotBeAcknowledged.Handle);
            }

            [Test]
            public void UnacknowledgedEventWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var @event = new TestEventTx(SingleValue);
                _bus.PublishTx(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            public void AcknowledgedEventWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var @event = new TestEventTx(SingleValue);
                _bus.PublishTx(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);
                _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

            [Test]
            public void UnacknowledgedEventIsUnaffectedByAcknowledgingOtherSubscriberAcrossAdditionalStarts()
            {
                var @event = new TestEventTx(SingleValue);
                _bus.PublishTx(@event);
                Assume.That(_eventsThatWillNotBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);

                for (int i = 0; i < 10; i++)
                {
                    _bus.Start();
                }

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_eventsThatWillNotBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }


        }

        [TestFixture]
        public class WhenSubscriberForPendingEventIsNoLongerRegistered
        {
            [Test]
            public void MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);

                var bus = new TransactionalMessageBus(retryPolicy);
                var events = new EventSubscribers();
                bus.RegisterSubscriptionFor<TestEventTx>(events.Handle);

                bus.PublishTx(new TestEventTx(singleValue));

                Assume.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Event Subscriber didn't receive the expected message.");

                bus.UnRegisterAllSubscriptionsFor<TestEventTx>();

                bus.Start();

                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Bus did not properly ignore queued event.");

            }
        }

        [TestFixture]
        public class WhenSubscriberForPendingCommandIsNoLongerRegistered
        {
            [Test]
            public void MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                
                var bus = new TransactionalMessageBus(retryPolicy);
                var commands = new CommandSubscribers();
                bus.RegisterSubscriptionFor<TestCommandTx>(commands.Handle);

                bus.PublishTx(new TestCommandTx(singleValue));

                Assume.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Command Subscriber didn't receive the expected message.");

                bus.UnRegisterAllSubscriptionsFor<TestCommandTx>();

                bus.Start();

                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Bus did not properly ignore queued command.");

            }
        }


        [TestFixture]
        public class MyClass
        {
             [Test]
             public void Test()
             {
                 const string singleValue = "0";
                 string doubleValue = string.Format("{0}{0}", singleValue);
                 string tripleValue = string.Format("{0}{0}{0}", singleValue);

                 var retryPolicy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
                 var bus = new TransactionalMessageBus(retryPolicy);

                 var events = new TransactionalEventSubscribers();

                 bus.RegisterSubscriptionFor<TestEventTx>(events.Handle);

                 bus.PublishTx(new TestEventTx(singleValue));

                 Assume.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Event Subscriber not registered for event as expected.");

                 bus.Start();

                 Assume.That(events.ProcessedMessagePayload, Is.EqualTo(doubleValue), "Event Subscriber not registered for event as expected.");

                 bus.Stop();

                 var savedCommands = bus.SerializedCommands;
                 var savedEvents = bus.SerializedEvents;

                 bus = null;

                 Assume.That(bus, Is.Null);

                 //recreate the bus from scratch
                 bus = new TransactionalMessageBus(retryPolicy);

                 //re-register the event subscriber
                 bus.RegisterSubscriptionFor<TestEventTx>(events.Handle);

                 bus.SerializedCommands = savedCommands;
                 bus.SerializedEvents = savedEvents;

                 //calling start should re-hydrate the list of pending (unacknowledged) events
                 // and then process them using the re-registered subscriber
                 bus.Start();

                 //we should now have one more payload element received
                 Assert.That(events.ProcessedMessagePayload, Is.EqualTo(tripleValue), "Event not properly re-hydrated.");

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
