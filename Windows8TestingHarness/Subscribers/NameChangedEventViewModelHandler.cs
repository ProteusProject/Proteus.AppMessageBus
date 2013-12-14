using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable;
using Windows8TestingHarness.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class NameChangedEventViewModelHandler : IHandle<NameChangedEvent>
    {
        public void Handle(NameChangedEvent message)
        {
            App.SetViewModelFor<DisplayPage>(new DisplayPageViewModel(message.Firstname, message.Lastname));
        }
    }
}