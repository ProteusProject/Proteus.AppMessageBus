using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Proteus.Infrastructure.Messaging.Portable;
using TestingHarness.Portable;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Subscribers;

namespace Windows10TestingHarness
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static DurableMessageBus Bus { get; private set; }
        public static IManageViewModels ViewModels { get; private set; }
        public static ObservableCollection<string> RunningLog { get; private set; }



        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            RunningLog = new ObservableCollection<string>();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Bus = new DurableMessageBus()
            {
                Logger = text =>
                {
                    Debug.WriteLine(text);
                    RunningLog.Add(text);
                }
            };

            ViewModels = new ViewModelManager();

            var registrar = new SubscriberRegistrar(Bus, ViewModels);
            registrar.RegisterMessageBusSubscribers();


            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var formattedMessage = string.Format("***Unobserved Task Exception: {0} ***", e.Exception);
            Debug.WriteLine(formattedMessage);
            RunningLog.Add(formattedMessage);
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.VisibilityChanged += CurrentWindowOnVisibilityChanged;
                
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(EditPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            
            //TODO: save any additional app state

            await Bus.Stop();

            deferral.Complete();
        }

        //TODO: determine whether its still necessary (or even advisable) to do this
        //(maybe we should just respond to the SUSPEND and RESUME events)
        private void CurrentWindowOnVisibilityChanged(object sender, VisibilityChangedEventArgs visibilityChangedEventArgs)
        {
            if (visibilityChangedEventArgs.Visible)
            {
                //no point in awaiting this, its in an event handler :)
                Bus.Start();
            }
            else
            {
                Bus.Stop();
            }
        }
    }
}
