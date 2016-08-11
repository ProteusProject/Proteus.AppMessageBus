using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows10TestingHarness.Common;
using TestingHarness.Portable.Messages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Windows10TestingHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : CommonBasePage
    {
        public EditPage()
        {
            this.InitializeComponent();
        }

     

        protected override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.Log.ItemsSource = App.RunningLog;

            UpdateLayout();
            this.Log.ScrollIntoView(this.Log.Items?[this.Log.Items.Count -1]);
        }

        #region Control Event Handlers

        private void SaveNames_OnClick(object sender, RoutedEventArgs e)
        {
            App.Bus.Send(new ChangeNameCommand(Firstname.Text, Lastname.Text));
            this.Frame.Navigate(typeof(DisplayPage));
        }

        private void IncrementCounterWithAck_OnClick(object sender, RoutedEventArgs e)
        {
            //send the command using default retries
            App.Bus.SendDurable(new IncrementCounterWithAckCommand());
            this.Frame.Navigate(typeof(CounterDisplayPage));
        }

        private void IncrementCounterWithoutAck_OnClick(object sender, RoutedEventArgs e)
        {
            //send the command using default retries
            App.Bus.SendDurable(new IncrementCounterWithoutAckCommand());
            this.Frame.Navigate(typeof(CounterDisplayPage));

        }

        private void NavigateToCounterDisplay_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CounterDisplayPage));
        }

        #endregion
    }
}
