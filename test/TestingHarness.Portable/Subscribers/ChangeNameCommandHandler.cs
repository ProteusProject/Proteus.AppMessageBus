using Proteus.AppMessageBus.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace TestingHarness.Portable.Subscribers
{
    public class ChangeNameCommandHandler : IHandle<ChangeNameCommand>
    {
        private readonly IPublishEvents _bus;

        public ChangeNameCommandHandler(IPublishEvents bus)
        {
            _bus = bus;
        }

        public void Handle(ChangeNameCommand message)
        {
            if (message.IsValidToHandle)
            {
                //once its actually completed, let anyone who cares know that this has happened
                _bus.Publish(new NameChangedEvent(message.NewFirstname, message.NewLastname));    
            }
        }
    }
}