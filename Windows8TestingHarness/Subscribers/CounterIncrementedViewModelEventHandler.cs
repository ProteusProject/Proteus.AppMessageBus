using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using Windows8TestingHarness.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class CounterIncrementedViewModelEventHandler
        : IHandleTransactional<CounterIncrementedWithAckEvent>, IHandleTransactional<CounterIncrementedWithoutAckEvent>
    {
        public void Handle(CounterIncrementedWithAckEvent message)
        {
            var viewModel = App.GetViewModelFor<CounterDisplayPage>() as CounterDisplayPageViewModel;

            if (null==viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.AcknowledgedCounter++;

            App.SetViewModelFor<CounterDisplayPage>(viewModel);

            App.Bus.Acknowledge(message);
        }

        public void Handle(CounterIncrementedWithoutAckEvent message)
        {
            var viewModel = App.GetViewModelFor<CounterDisplayPage>() as CounterDisplayPageViewModel;

            if (null == viewModel)
            {
                viewModel = new CounterDisplayPageViewModel();
            }

            viewModel.UnacknowledgedCounter++;

            App.SetViewModelFor<CounterDisplayPage>(viewModel);

            //intentionally do NOT acknowledge the message
            //App.Bus.Acknowledge(message);
        }
    }

    
}