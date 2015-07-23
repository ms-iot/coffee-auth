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
        User lastUser;

        public MainPage()
        {
            this.InitializeComponent();
            // set fullscreen
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // last user
            lastUser = e.Parameter as User;

            var users = DrinkerDatabase.Instance.GetAllUsers();
            users.Sort(
                delegate (User u1, User u2)
                {
                    return u1.Balance.CompareTo(u2.Balance);
                }
             );

            listView.ItemsSource = users;

            if (lastUser != null)
            {
                int idx = -1;
                foreach (var swag in users)
                {
                    if (swag.BadgeCIN == lastUser.BadgeCIN)
                    {
                        idx = users.IndexOf(swag);
                        break;
                    }
                }

                try
                {
                    var selected = listView.Items[idx];
                    listView.SelectedIndex = users.IndexOf(lastUser);
                    listView.SelectedItem = selected;
                }
                catch
                {

                }

            }
        }

        private void coffee_Click(object sender, RoutedEventArgs e)
        {
            if (badgeCIN.Text.Length == 16)
            {
                Frame.Navigate(typeof(UserPage), badgeCIN.Text);
            }
            else if (badgeCIN.Text == App.rl.GetString("AdminPanelPassword"))
            {
                Frame.Navigate(typeof(AdminPage));
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
