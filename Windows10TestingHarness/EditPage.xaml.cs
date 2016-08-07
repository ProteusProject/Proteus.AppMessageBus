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
using AppUIBasics.Common;
using TestingHarness.Portable.Messages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Windows10TestingHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            this.InitializeComponent();
            #region move to common base class
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
            #endregion
        }

        #region Move to common base class

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Get a hold of the current frame so that we can inspect the app back stack.

            if (this.Frame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (this.Frame.CanGoBack)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                e.Handled = true;
                this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }


        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="AppUIBasics.Common.NavigationHelper.LoadState"/>
        /// and <see cref="AppUIBasics.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = this.Frame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.Log.ItemsSource = App.RunningLog;

            UpdateLayout();
            this.Log.SelectedIndex = App.RunningLog.Count - 1;
            this.Log.ScrollIntoView(this.Log.SelectedItem);
        }

        #endregion

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
