using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Windows8TestingHarness.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class NameChangedEventHandler : IHandle<NameChangedEvent>
    {
        public void Handle(NameChangedEvent message)
        {
            var viewModel = new DisplayPageViewModel(message.Firstname, message.Lastname);
            App.SetViewModelFor<DisplayPage>(viewModel);
        }
    }
}