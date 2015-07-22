using CoffeeAuth.Models;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CoffeeAuth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page 
    {

        public MainPage()
        {
            this.InitializeComponent();

            // set fullscreen
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var users = DrinkerDatabase.Instance.GetAllUsers();
            listView.ItemsSource = users;
        }

        private void coffee_Click(object sender, RoutedEventArgs e)
        {
            if (badgeCIN.Text.Length != 0)
            {
                this.Frame.Navigate(typeof(UserPage), badgeCIN.Text);
            }
        }

        private async void badgeCIN_LostFocus(object sender, RoutedEventArgs e)
        {
            // Don't refocus immediately to allow interaction with UI
            await Task.Delay(1000);
            badgeCIN.Focus(FocusState.Programmatic);
        }

    }
}
