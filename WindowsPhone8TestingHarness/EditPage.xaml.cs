using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TestingHarness.Portable.Messages;

namespace WindowsPhone8TestingHarness
{
    public partial class EditPage : PhoneApplicationPage
    {
        public EditPage()
        {
            InitializeComponent();
        }

        private void SaveNames_OnClick(object sender, RoutedEventArgs e)
        {
            App.Bus.Send(new ChangeNameCommand(Firstname.Text, Lastname.Text));
            NavigationService.Navigate(new Uri("/DisplayPage.xaml", UriKind.Relative));
        }

        private void IncrementCounterWithAck_OnClick(object sender, RoutedEventArgs e)
        {
            //send the command using default retries
            App.Bus.SendDurable(new IncrementCounterWithAckCommand());
            NavigationService.Navigate(new Uri("/CounterDisplayPage.xaml", UriKind.Relative));
        }

        private void IncrementCounterWithoutAck_OnClick(object sender, RoutedEventArgs e)
        {
            //send the command using default retries
            App.Bus.SendDurable(new IncrementCounterWithoutAckCommand());
            NavigationService.Navigate(new Uri("/CounterDisplayPage.xaml", UriKind.Relative));
        }
    }
}