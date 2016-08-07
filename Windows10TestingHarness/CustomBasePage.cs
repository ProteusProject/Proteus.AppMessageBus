using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using AppUIBasics.Common;

namespace Windows10TestingHarness
{
    public abstract class CustomBasePage : Page
    {
        protected NavigationHelper navigationHelper;
        protected ObservableDictionary defaultViewModel = new ObservableDictionary();


        public CustomBasePage()
        {
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
            
        }

        protected virtual void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
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
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="AppUIBasics.Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        protected virtual void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        #region NavigationHelper registration

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
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = this.Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}