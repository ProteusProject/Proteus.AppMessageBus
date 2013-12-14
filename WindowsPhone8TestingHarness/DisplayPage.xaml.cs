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
    public partial class DisplayPage : PhoneApplicationPage
    {
        public DisplayPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var nameValues = App.ViewModelManager.RetrieveViewModel<DisplayPageViewModel>() ??
                             new DisplayPageViewModel("Unknown", "Unknown");

            DataContext = nameValues;
        }
    }
}