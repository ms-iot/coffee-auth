using CoffeeAuth.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            var users = GetAllUsers();
            listView.ItemsSource = users;
        }

        private void coffee_Click(object sender, RoutedEventArgs e)
        {
            if (badgeCIN.Text.Length != 0)
                this.Frame.Navigate(typeof(UserPage), badgeCIN.Text);
        }

        private List<string> GetAllUsers()
        {
            List<string> users = new List<string>();
            using (var statement = App.conn.Prepare("SELECT Name, Balance, BadgeCIN FROM Customer"))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    var user = new User()
                    {
                        Name = (string)statement[0],
                        Balance = (long)statement[1],
                        BadgeCIN = (string)statement[2]
                    };
                    users.Add(user.Name + ": " + user.Balance);
                }
            }
            return users;
        }

        private async void badgeCIN_LostFocus(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            this.badgeCIN.Focus(FocusState.Programmatic);
        }
    }
}
