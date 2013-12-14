using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TestingHarness.Portable.ViewModels;

namespace WindowsPhone8TestingHarness
{
    public partial class CounterDisplayPage : PhoneApplicationPage
    {
        public CounterDisplayPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var viewModel = App.ViewModelManager.RetrieveViewModel<CounterDisplayPageViewModel>() ??
                            new CounterDisplayPageViewModel {AcknowledgedCounter = 0, UnacknowledgedCounter = 0};

            DataContext = viewModel;
            //base.OnNavigatedTo(e);
        }
    }
}