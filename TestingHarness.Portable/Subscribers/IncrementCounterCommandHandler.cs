using System;
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;
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

        public async void Handle(IncrementCounterWithAckCommand message)
        {
            //publish the event with some retries and a future expiry
            await _bus.PublishDurable(new CounterIncrementedWithAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            await _bus.Acknowledge(message);
        }

        public async void Handle(IncrementCounterWithoutAckCommand message)
        {
            //publish the event with some retries and a future expiry
            await _bus.PublishDurable(new CounterIncrementedWithoutAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            await _bus.Acknowledge(message);
        }
    }
}