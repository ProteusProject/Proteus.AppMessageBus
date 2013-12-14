using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace TestingHarness.Portable.Messages
{
    public class NameChangedEvent : Event
    {
        public string Firstname { get;  private set; }
        public string Lastname { get;  private set; }

        public NameChangedEvent(string firstname, string lastname)
        {
            Firstname = firstname;
            Lastname = lastname;
        }
    }
}