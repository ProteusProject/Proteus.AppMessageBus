using System;
using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Windows8TestingHarness.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class IncrementCounterCommandHandler 
        : IHandleDurable<IncrementCounterWithAckCommand>, IHandleDurable<IncrementCounterWithoutAckCommand>
    {
        public void Handle(IncrementCounterWithAckCommand message)
        {
            //publish the event with some retries and a future expiry
            App.Bus.PublishTx(new CounterIncrementedWithAckEvent(), new RetryPolicy(2, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            App.Bus.Acknowledge(message);
        }

        public void Handle(IncrementCounterWithoutAckCommand message)
        {
            //publish the event with some retries and a future expiry
            App.Bus.PublishTx(new CounterIncrementedWithoutAckEvent(), new RetryPolicy(2, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            App.Bus.Acknowledge(message);
        }
    }
}