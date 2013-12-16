using System;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace TestingHarness.Portable.Subscribers
{
    public class IncrementCounterCommandHandler 
        : IHandleDurable<IncrementCounterWithAckCommand>, IHandleDurable<IncrementCounterWithoutAckCommand>
    {

        private readonly DurableMessageBus _bus;

        public IncrementCounterCommandHandler(DurableMessageBus bus)
        {
            _bus = bus;
        }

        public void Handle(IncrementCounterWithAckCommand message)
        {
            //publish the event with some retries and a future expiry
            _bus.PublishDurable(new CounterIncrementedWithAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            _bus.Acknowledge(message);
        }

        public void Handle(IncrementCounterWithoutAckCommand message)
        {
            //publish the event with some retries and a future expiry
            _bus.PublishDurable(new CounterIncrementedWithoutAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            _bus.Acknowledge(message);
        }
    }
}