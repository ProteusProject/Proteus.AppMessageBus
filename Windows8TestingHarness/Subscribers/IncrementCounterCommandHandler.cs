using System;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class IncrementCounterCommandHandler 
        : IHandleDurable<IncrementCounterWithAckCommand>, IHandleDurable<IncrementCounterWithoutAckCommand>
    {
        public void Handle(IncrementCounterWithAckCommand message)
        {
            //publish the event with some retries and a future expiry
            App.Bus.PublishDurable(new CounterIncrementedWithAckEvent(), new RetryPolicy(2, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            App.Bus.Acknowledge(message);
        }

        public void Handle(IncrementCounterWithoutAckCommand message)
        {
            //publish the event with some retries and a future expiry
            App.Bus.PublishDurable(new CounterIncrementedWithoutAckEvent(), new RetryPolicy(2, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            App.Bus.Acknowledge(message);
        }
    }
}