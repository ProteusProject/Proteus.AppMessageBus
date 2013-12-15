using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Messages;
using TestingHarness.Portable.ViewModels;

namespace TestingHarness.Portable.Subscribers
{
    public class CounterIncrementedViewModelEventHandler
        : IHandleDurable<CounterIncrementedWithAckEvent>, IHandleDurable<CounterIncrementedWithoutAckEvent>
    {

        private readonly DurableMessageBus _bus;
        private readonly IManageViewModels _modelManager;

        public CounterIncrementedViewModelEventHandler(DurableMessageBus bus, IManageViewModels modelManager)
        {
            _bus = bus;
            _modelManager = modelManager;
        }

        public void Handle(CounterIncrementedWithAckEvent message)
        {
            var viewModel = _modelManager.Get<CounterDisplayPageViewModel>();

            if (null == viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.AcknowledgedCounter++;

            _modelManager.Put(viewModel);

            _bus.Acknowledge(message);
        }

        public void Handle(CounterIncrementedWithoutAckEvent message)
        {
            var viewModel = _modelManager.Get<CounterDisplayPageViewModel>();

            if (null == viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.UnacknowledgedCounter++;

            _modelManager.Put(viewModel);

            //intentionally do NOT acknowledge the message...that's the whole pt of this handler :)
            //_bus.Acknowledge(message);
        }
    }


}