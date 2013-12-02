using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Windows8TestingHarness.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class NameChangedEventPersistenceHandler : IHandle<NameChangedEvent>
    {
        public void Handle(NameChangedEvent message)
        {
            //TODO: perform actual persistence here in this handler
        }
    }
}