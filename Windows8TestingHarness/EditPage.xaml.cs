using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Proteus.Infrastructure.Messaging.Portable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows8TestingHarness.Common;
using Windows8TestingHarness.Messages;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Windows8TestingHarness
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class EditPage : Windows8TestingHarness.Common.LayoutAwarePage
    {
        public EditPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

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
    }
}
