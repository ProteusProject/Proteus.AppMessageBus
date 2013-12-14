using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable;
using TestingHarness.Portable.Messages;
using TestingHarness.Portable.ViewModels;

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