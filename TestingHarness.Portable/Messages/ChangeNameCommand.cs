using Proteus.AppMessageBus.Portable.Abstractions;

namespace TestingHarness.Portable.Messages
{
    public class ChangeNameCommand : Command
    {
        public string NewFirstname { get; private set; }
        public string NewLastname { get; private set; }

        public bool IsValidToHandle 
        {
            get { return !string.IsNullOrEmpty(NewFirstname) && !string.IsNullOrEmpty(NewLastname); } 
        }

        public ChangeNameCommand(string newFirstname, string newLastname)
        {
            NewFirstname = newFirstname;
            NewLastname = newLastname;
        }
    }
}