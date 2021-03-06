﻿using System;
using System.Collections.Generic;
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
using TestingHarness.Portable.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Windows10TestingHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CounterDisplayPage : CommonBasePage
    {
        public CounterDisplayPage()
        {
            this.InitializeComponent();
        }

        protected override void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var viewModel = App.ViewModels.Get<CounterDisplayPageViewModel>();

            this.DefaultViewModel["AcknowledgedCounter"] = null != viewModel ? viewModel.AcknowledgedCounter : 0;
            this.DefaultViewModel["UnacknowledgedCounter"] = null != viewModel ? viewModel.UnacknowledgedCounter : 0;
        }

    }
}
