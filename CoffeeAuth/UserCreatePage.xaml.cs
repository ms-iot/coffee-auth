using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CoffeeAuth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserCreatePage : Page
    {
        private string badgeCIN;

        public UserCreatePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            badgeCIN = e.Parameter as string;
        }

        public void createUser()
        {
            var db = App.conn;

            try
            {
                using (var userstmt = db.Prepare("INSERT INTO Customer (Name, BadgeCIN, BALANCE) VALUES (?, ?, ?)"))
                {
                    userstmt.Bind(1, userTextBox.Text);
                    userstmt.Bind(2, badgeCIN);
                    userstmt.Bind(3, 0); // initial balance of zero
                    userstmt.Step();
                }
            }
            catch
            {
                // handle error
            }
        }

        private void createUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userTextBox.Text.Length != 0)
                createUser();
            this.Frame.Navigate(typeof(UserPage), badgeCIN);
        }
    }

}
