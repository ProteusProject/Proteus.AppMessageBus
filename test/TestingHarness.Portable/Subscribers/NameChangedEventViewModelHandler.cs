using Proteus.AppMessageBus.Portable.Abstractions;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Messages;
using TestingHarness.Portable.ViewModels;

namespace TestingHarness.Portable.Subscribers
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
            _modelManager.Put(new DisplayPageViewModel(message.Firstname, message.Lastname));
        }
    }
}