using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PCLStorage;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Tests
{
    public class DurableMessageBusTests
    {
        public static async Task ClearAllDataFiles()
        {
            var messagePersistence = new MesssagePersistence();
            await messagePersistence.RemoveAllCommandsFromPersistence();
            await messagePersistence.RemoveAllEventsFromPersistence();
        }

        [TestFixture]
        public class WhenConfiguredWithNonZeroEventRetryAndCommandRetryAndMessagesHaveNotExpired
        {
            private DurableMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private readonly string _doubleValue = String.Format("{0}{0}", SingleValue);
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();

                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new DurableMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);

            }

            [Test]
            public async Task CommandAndEventAreRetriedOnNextStart()
            {
                await _bus.SendDurable(new TestDurableCommand(SingleValue));
                await _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

            [Test]
            public async Task CommandAndEventRetriesRespectRetryPolicyAcrossAdditionalStarts()
            {
                await _bus.SendDurable(new TestDurableCommand(SingleValue));
                await _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                //despite multiple calls to Start(), messages are only retried ONCE as per the retry policy setting
                for (int i = 0; i < 10; i++)
                {
                    await _bus.Start();
                }

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

        }

        [TestFixture]
        public class WhenConfiguredWithZeroEventRetryAndZeroCommandRetry
        {
            private DurableMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private readonly string _doubleValue = String.Format("{0}{0}", SingleValue);
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();

                var retryPolicy = new RetryPolicy();
                Assume.That(retryPolicy.Retries, Is.EqualTo(0));

                _bus = new DurableMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);
            }

            [Test]
            public async Task CommandAndEventAreNotRetriedAcrossAdditionalStarts()
            {
                await _bus.SendDurable(new TestDurableCommand(SingleValue));
                await _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                //despite multiple calls to Start(), messages are only retried ONCE as per the retry policy setting
                for (int i = 0; i < 10; i++)
                {
                    await _bus.Start();
                }

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }
        }

        [TestFixture]
        public class WhenConfiguredWithNonZeroEventRetryAndNonZeroCommandRetryAndMessagesHaveAlreadyExpired
        {
            private DurableMessageBus _bus;
            private CommandSubscribers _commands;
            private EventSubscribers _events;
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();

                var retryPolicy = new RetryPolicy(1, DateTimeUtility.NegativeOneHourTimeSpan);
                _bus = new DurableMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _events = new EventSubscribers();
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);
            }

            [Test]
            public async Task CommandAndEventAreNotRetriedOnNextStart()
            {
                await _bus.SendDurable(new TestDurableCommand(SingleValue));
                await _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

        }


        [TestFixture]
        public class WhenAcknowledgingCommands
        {
            private DurableMessageBus _bus;
            private DurableCommandSubscribers _commands;
            private DurableEventSubscribers _events;
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();

                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new DurableMessageBus(retryPolicy);

                _commands = new DurableCommandSubscribers();
                _events = new DurableEventSubscribers();
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);
            }

            [Test]
            public async Task UnacknowledgedCommandWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var command = new TestDurableCommand(SingleValue);
                await _bus.SendDurable(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            public async Task AcknowledgedCommandWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var command = new TestDurableCommand(SingleValue);
                await _bus.SendDurable(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _commands.AcknowledgeLastMessage(_bus);
                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }
        }

        [TestFixture]
        public class WhenAcknowledgingEvents
        {
            private DurableMessageBus _bus;
            private CommandSubscribers _commands;
            private DurableEventSubscribers _eventsThatWillBeAcknowledged;
            private DurableEventSubscribers _eventsThatWillNotBeAcknowledged;
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();

                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new DurableMessageBus(retryPolicy);

                _commands = new CommandSubscribers();
                _eventsThatWillBeAcknowledged = new DurableEventSubscribers();
                _eventsThatWillNotBeAcknowledged = new DurableEventSubscribers();
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_eventsThatWillBeAcknowledged.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_eventsThatWillNotBeAcknowledged.Handle);
            }

            [Test]
            public async Task UnacknowledgedEventWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                await _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            public async Task AcknowledgedEventWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                await _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);
                await _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

            [Test]
            public async Task UnacknowledgedEventIsUnaffectedByAcknowledgingOtherSubscriberAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                await _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillNotBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);

                for (int i = 0; i < 10; i++)
                {
                    await _bus.Start();
                }

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assert.That(_eventsThatWillNotBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }


        }

        [TestFixture]
        public class WhenSubscriberForPendingEventIsNoLongerRegistered
        {

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();
            }

            [Test]
            public async Task MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);

                var bus = new DurableMessageBus(retryPolicy);
                var events = new EventSubscribers();
                bus.RegisterSubscriptionFor<TestDurableEvent>(events.Handle);

                await bus.PublishDurable(new TestDurableEvent(singleValue));

                Assume.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Event Subscriber didn't receive the expected message.");

                bus.UnRegisterAllSubscriptionsFor<TestDurableEvent>();

                await bus.Start();

                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Bus did not properly ignore queued event.");

            }
        }

        [TestFixture]
        public class WhenSubscriberForPendingCommandIsNoLongerRegistered
        {
            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();
            }

            [Test]
            public async Task MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);

                var bus = new DurableMessageBus(retryPolicy);
                var commands = new CommandSubscribers();
                bus.RegisterSubscriptionFor<TestDurableCommand>(commands.Handle);

                await bus.SendDurable(new TestDurableCommand(singleValue));

                Assume.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Command Subscriber didn't receive the expected message.");

                bus.UnRegisterAllSubscriptionsFor<TestDurableCommand>();

                await bus.Start();

                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Bus did not properly ignore queued command.");

            }
        }


        [TestFixture]
        public class WhenStartingTheBusWithPersistedUnacknowledgedCommandsAndEvents
        {
            private string _doubleValue;
            private string _tripleValue;
            private DurableMessageBus _bus;
            private RetryPolicy _retryPolicy;
            private DurableEventSubscribers _events;
            private DurableCommandSubscribers _commands;
            private const string SingleValue = "0";

            [SetUp]
            public async Task SetUp()
            {
                await ClearAllDataFiles();


            }

            private async Task SendUnacknowlegedCommandAndEventTwiceThenDisposeDurableBus()
            {
                _doubleValue = string.Format("{0}{0}", SingleValue);
                _tripleValue = string.Format("{0}{0}{0}", SingleValue);

                _retryPolicy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
                _bus = new DurableMessageBus(_retryPolicy);

                _commands = new DurableCommandSubscribers();
                _events = new DurableEventSubscribers();

                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);

                await _bus.SendDurable(new TestDurableCommand(SingleValue));
                await _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue),
                            "Command Subscriber not registered for command as expected.");
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue),
                            "Event Subscriber not registered for event as expected.");

                await _bus.Start();

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue),
                            "Command Subscriber not registered for command as expected.");
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue),
                            "Event Subscriber not registered for event as expected.");

                await _bus.Stop();

                _bus = null;

                Assume.That(_bus, Is.Null);
            }

            [Test]
            public async Task CanRepublishDurableEventsOnNextStart()
            {
                await SendUnacknowlegedCommandAndEventTwiceThenDisposeDurableBus();

                //recreate the bus from scratch
                _bus = new DurableMessageBus(_retryPolicy);

                //re-register the event subscriber
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);

                //calling start should re-hydrate the list of pending (unacknowledged) events
                // and then process them using the re-registered subscriber
                await _bus.Start();

                //we should now have one more payload element received
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_tripleValue), "Event not properly re-hydrated.");
            }

            [Test]
            public async Task CanRepublishDurableCommandsOnNextStart()
            {
                await SendUnacknowlegedCommandAndEventTwiceThenDisposeDurableBus();

                //recreate the bus from scratch
                _bus = new DurableMessageBus(_retryPolicy);

                //re-register the command subscriber
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);

                //calling start should re-hydrate the list of pending (unacknowledged) commands
                // and then process them using the re-registered subscriber
                await _bus.Start();

                //we should now have one more payload element received
                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_tripleValue), "Command not properly re-hydrated.");
            }

            [Test]
            public async Task DurableCommandsWithoutMatchingVersionAreDiscaredOnBusStart()
            {
                await SendUnacknowlegedCommandAndEventTwiceThenDisposeDurableBus();

                //recreate the bus from scratch, set the version to be something *other* than the default empty string
                _bus = new DurableMessageBus(_retryPolicy) { MessageVersionProvider = () => "not-the-default" };

                //re-register the command subscriber
                _bus.RegisterSubscriptionFor<TestDurableCommand>(_commands.Handle);

                //calling start should re-hydrate the list of pending (unacknowledged) commands
                // and then process them using the re-registered subscriber
                await _bus.Start();

                //becasue the version of the command doesn't match, we should still only have original two payload elements received
                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue), "Command with wrong version not properly ignored.");
            }

            [Test]
            public async Task DurableEventsWithoutMatchingVersionAreDiscaredOnBusStart()
            {
                await SendUnacknowlegedCommandAndEventTwiceThenDisposeDurableBus();

                //recreate the bus from scratch, set the version to be something *other* than the default empty string
                _bus = new DurableMessageBus(_retryPolicy) { MessageVersionProvider = () => "not-the-default" };

                //re-register the event subscriber
                _bus.RegisterSubscriptionFor<TestDurableEvent>(_events.Handle);

                //calling start should re-hydrate the list of pending (unacknowledged) events
                // and then process them using the re-registered subscriber
                await _bus.Start();

                //becasue the version of the command doesn't match, we should still only have original two payload elements received
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue), "Event with wrong version not properly ignored.");
            }
        }

        [TestFixture]
        public class WhenProcessingMessageVersion
        {
            public class VersionProvider
            {
                public string Version { get; set; }
            }

            [Test]
            public void DefaultValueIsEmptyString()
            {
                var bus = new DurableMessageBus(new RetryPolicy());

                Assert.That(bus.MessageVersion, Is.Not.Null);
                Assert.That(bus.MessageVersion, Is.Empty);
            }

            [Test]
            public void VersionIsCachedAfterInitialCalculationAndNotRecalculated()
            {
                const string expected = "1";
                const string notExpected = "2";

                var versionProvider = new VersionProvider { Version = expected };

                var bus = new DurableMessageBus(new RetryPolicy())
                    {
                        MessageVersionProvider = () => versionProvider.Version
                    };

                Assume.That(bus.MessageVersion, Is.EqualTo(expected), "MessageVersionProvider delegate not wired up properly.");

                //this changes the value thta would be returned by the delegate, should it improperly be invoked a second time
                versionProvider.Version = notExpected;

                //since the delegate should NOT be invoked a second time, we expect the original value to be retained
                Assert.That(bus.MessageVersion, Is.EqualTo(expected));

            }


        }


        public class DurableEventSubscribers : DurableSubscribers, IHandleDurable<TestDurableEvent>
        {
            public string ProcessedMessagePayload { get; private set; }
            public IList<Tuple<Guid, Guid>> ProcessedMessageIds { get; private set; }

            public void Handle(TestDurableEvent message)
            {
                ProcessedMessagePayload += message.Payload;
                ProcessedMessageIds.Add(new Tuple<Guid, Guid>(message.Id, message.AcknowledgementId));
                Messages.Add(message);
            }

            public DurableEventSubscribers()
            {
                ProcessedMessageIds = new List<Tuple<Guid, Guid>>();
            }
        }

        public class DurableSubscribers
        {
            public async Task AcknowledgeLastMessage(DurableMessageBus bus)
            {
                await bus.Acknowledge(Messages.Last());
            }

            public IList<IDurableMessage> Messages { get; private set; }

            public DurableSubscribers()
            {
                Messages = new List<IDurableMessage>();
            }
        }

        public class DurableCommandSubscribers : DurableSubscribers, IHandleDurable<TestDurableCommand>
        {
            public string ProcessedMessagePayload { get; private set; }
            public IList<Tuple<Guid, Guid>> ProcessedMessageIds { get; private set; }

            public void Handle(TestDurableCommand message)
            {
                ProcessedMessagePayload += message.Payload;
                ProcessedMessageIds.Add(new Tuple<Guid, Guid>(message.Id, message.AcknowledgementId));
                Messages.Add(message);
            }

            public DurableCommandSubscribers()
            {
                ProcessedMessageIds = new List<Tuple<Guid, Guid>>();
            }
        }

        public class TestDurableEvent : TestEvent, IDurableEvent
        {
            public TestDurableEvent(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }


        public class TestDurableCommand : TestCommand, IDurableCommand
        {
            public TestDurableCommand(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }

    }
}
