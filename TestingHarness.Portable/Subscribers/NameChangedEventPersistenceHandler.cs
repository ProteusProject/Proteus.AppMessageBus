using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace TestingHarness.Portable.Subscribers
{
    public class NameChangedEventPersistenceHandler : IHandle<NameChangedEvent>
    {
        public void Handle(NameChangedEvent message)
        {
            //TODO: perform actual persistence here in this handler
        }
    }
}