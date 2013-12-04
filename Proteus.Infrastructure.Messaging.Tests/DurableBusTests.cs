using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PCLStorage;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public class DurableBusTests
    {
        public static async Task ClearAllDataFiles()
        {
            var fileProvider = new FileSystemProvider();
            await fileProvider.DeleteFolderAsync(FileSystem.Current.LocalStorage, "Proteus.Messaging.Messages");
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
            async public void SetUp()
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
            async public void CommandAndEventAreRetriedOnNextStart()
            {
                _bus.SendDurable(new TestDurableCommand(SingleValue));
                _bus.PublishDurable(new TestDurableEvent(SingleValue));

                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));
                Assume.That(_events.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
                Assert.That(_events.ProcessedMessagePayload, Is.EqualTo(_doubleValue));
            }

            [Test]
            async public void CommandAndEventRetriesRespectRetryPolicyAcrossAdditionalStarts()
            {
                _bus.SendDurable(new TestDurableCommand(SingleValue));
                _bus.PublishDurable(new TestDurableEvent(SingleValue));

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
            async public void SetUp()
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
            async public void CommandAndEventAreNotRetriedAcrossAdditionalStarts()
            {
                _bus.SendDurable(new TestDurableCommand(SingleValue));
                _bus.PublishDurable(new TestDurableEvent(SingleValue));

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
            async public void SetUp()
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
            async public void CommandAndEventAreNotRetriedOnNextStart()
            {
                _bus.SendDurable(new TestDurableCommand(SingleValue));
                _bus.PublishDurable(new TestDurableEvent(SingleValue));

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
            async public void SetUp()
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
            async public void UnacknowledgedCommandWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var command = new TestDurableCommand(SingleValue);
                _bus.SendDurable(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_commands.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            async public void AcknowledgedCommandWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var command = new TestDurableCommand(SingleValue);
                _bus.SendDurable(command);
                Assume.That(_commands.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _commands.AcknowledgeLastMessage(_bus);
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
            async public void SetUp()
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
            async public void UnacknowledgedEventWithNonExpiredRetryPolicyIsRetriedAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                await _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(string.Format("{0}{0}", SingleValue)));
            }

            [Test]
            async public void AcknowledgedEventWithNonExpiredRetryPolicyIsNotRetriedAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);
                await _bus.Start();

                Assert.That(_eventsThatWillBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));
            }

            [Test]
            async public void UnacknowledgedEventIsUnaffectedByAcknowledgingOtherSubscriberAcrossAdditionalStarts()
            {
                var @event = new TestDurableEvent(SingleValue);
                _bus.PublishDurable(@event);
                Assume.That(_eventsThatWillNotBeAcknowledged.ProcessedMessagePayload, Is.EqualTo(SingleValue));

                _eventsThatWillBeAcknowledged.AcknowledgeLastMessage(_bus);

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
            async public void SetUp()
            {
                await ClearAllDataFiles();
            }

            [Test]
            async public void MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);

                var bus = new DurableMessageBus(retryPolicy);
                var events = new EventSubscribers();
                bus.RegisterSubscriptionFor<TestDurableEvent>(events.Handle);

                bus.PublishDurable(new TestDurableEvent(singleValue));

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
            async public void SetUp()
            {
                await ClearAllDataFiles();
            }

            [Test]
            async public void MessageIsNotSendToSubscriberOnStart()
            {
                const string singleValue = "0";

                //we need a retry policy with at least one retry
                //  so that we'd expect the call to Start() to attemp a retry
                var retryPolicy = new RetryPolicy(1, DateTimeUtility.PositiveOneHourTimeSpan);

                var bus = new DurableMessageBus(retryPolicy);
                var commands = new CommandSubscribers();
                bus.RegisterSubscriptionFor<TestDurableCommand>(commands.Handle);

                bus.PublishDurable(new TestDurableCommand(singleValue));

                Assume.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Command Subscriber didn't receive the expected message.");

                bus.UnRegisterAllSubscriptionsFor<TestDurableCommand>();

                await bus.Start();

                Assert.That(commands.ProcessedMessagePayload, Is.EqualTo(singleValue), "Bus did not properly ignore queued command.");

            }
        }


        [TestFixture]
        public class MyClass
        {
            [SetUp]
            async public void SetUp()
            {
                await ClearAllDataFiles();
            }

            [Test]
            async public void Test()
            {
                const string singleValue = "0";
                string doubleValue = string.Format("{0}{0}", singleValue);
                string tripleValue = string.Format("{0}{0}{0}", singleValue);

                var retryPolicy = new RetryPolicy(10, DateTimeUtility.PositiveOneHourTimeSpan);
                var bus = new DurableMessageBus(retryPolicy);

                var events = new DurableEventSubscribers();

                bus.RegisterSubscriptionFor<TestDurableEvent>(events.Handle);

                bus.PublishDurable(new TestDurableEvent(singleValue));

                Assume.That(events.ProcessedMessagePayload, Is.EqualTo(singleValue), "Event Subscriber not registered for event as expected.");

                await bus.Start();

                Assume.That(events.ProcessedMessagePayload, Is.EqualTo(doubleValue), "Event Subscriber not registered for event as expected.");

                await bus.Stop();

                //capture the results of the serialization so that we can pass them back to the bus later
                //var savedCommands = bus.SerializedCommands;
                //var savedEvents = bus.SerializedEvents;

                bus = null;

                Assume.That(bus, Is.Null);

                //recreate the bus from scratch
                bus = new DurableMessageBus(retryPolicy);

                //re-register the event subscriber
                bus.RegisterSubscriptionFor<TestDurableEvent>(events.Handle);

                //bus.SerializedCommands = savedCommands;
                //bus.SerializedEvents = savedEvents;

                //calling start should re-hydrate the list of pending (unacknowledged) events
                // and then process them using the re-registered subscriber
                await bus.Start();

                //we should now have one more payload element received
                Assert.That(events.ProcessedMessagePayload, Is.EqualTo(tripleValue), "Event not properly re-hydrated.");
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

                //this changes the value returned by the delegate, should it improperly be invoked a second time
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
            public void AcknowledgeLastMessage(DurableMessageBus bus)
            {
                bus.Acknowledge(Messages.Last());
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

        public class TestDurableEvent : TestEvent, IDurableMessage
        {
            public TestDurableEvent(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }


        public class TestDurableCommand : TestCommand, IDurableMessage
        {
            public TestDurableCommand(string payload)
                : base(payload)
            {
            }

            public Guid AcknowledgementId { get; set; }
        }

    }
}
