using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Messages;
using TestingHarness.Portable.ViewModels;

namespace Windows8TestingHarness.Subscribers
{
    public class NameChangedEventViewModelHandler : IHandle<NameChangedEvent>
    {
        private readonly IManageViewModels _modelManager;

        public NameChangedEventViewModelHandler(IManageViewModels modelManager)
        {
            _modelManager = modelManager;
        }

        public void Handle(NameChangedEvent message)
        {
            _modelManager.SetViewModelFor<DisplayPage>(new DisplayPageViewModel(message.Firstname, message.Lastname));
        }
    }
}