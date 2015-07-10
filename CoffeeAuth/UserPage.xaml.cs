using CoffeeAuth.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
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
    public sealed partial class UserPage : Page
    {
        private string badgeCIN;
        private User m_user;

        public UserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            badgeCIN = e.Parameter as string;


            // check if user is in the database
            m_user = GetUser(badgeCIN);
            if (m_user == null)
            {
                // create user
                var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
                {
                    this.Frame.Navigate(typeof(UserCreatePage), badgeCIN);
                }
                ));
            }
            else
            {
                // Show user profile
                userName.Text = m_user.Name;
                userBalance.Text = m_user.Balance.ToString();
            }
        }

        private User GetUser(string badgeCIN)
        {
            User user = null;

            using (var statement = App.conn.Prepare("SELECT BadgeCIN, Name, Balance FROM Customer WHERE BadgeCIN = ?"))
            {
                statement.Bind(1, badgeCIN);
                if (SQLiteResult.ROW == statement.Step())
                {
                        user = new User()
                        {
                            BadgeCIN = (string)statement[0],
                            Name = (string)statement[1],
                            Balance = (long)statement[2]
                        };
                }
            }
            return user;
        }

        private void UpdateUserBalance(User user)
        {
            var existingUser = GetUser(badgeCIN);
            if (existingUser != null)
            {
                using (var custstmt = App.conn.Prepare("UPDATE Customer SET Balance = ? WHERE BadgeCIN=?"))
                {
                    custstmt.Bind(1, user.Balance);
                    custstmt.Bind(2, user.BadgeCIN);
                    custstmt.Step();
                }
            }
        }

        private void getCoffeeButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Espresso Shot", "$1 debited from your account", "thank you for shopping!");
            m_user.Balance--;
            userBalance.Text = m_user.Balance.ToString();
            UpdateUserBalance(m_user);
            this.Frame.Navigate(typeof(MainPage));

            // todo: turn on grinder for around 1 minute

        }

        private void bagButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Espresso Beans Deposited", "$14 credited to your account.", "Everyone appreciates your efforts");
            m_user.Balance += 14;
            userBalance.Text = m_user.Balance.ToString();
            UpdateUserBalance(m_user);
        }

        private void milkButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Milk Jug Deposited", "$6 credited to your account.", "Everyone appreciates your efforts");
            m_user.Balance += 6;
            userBalance.Text = m_user.Balance.ToString();
            UpdateUserBalance(m_user);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }



        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        private void showToast(string heading, string body, string body2)

        {
            var builder = new StringBuilder();
            builder.Append("<toast><visual version='1'><binding template='ToastText04'><text id='1'>")
                .Append(heading)
                .Append("</text><text id='2'>")
                .Append(body)
                .Append("</text>");

            if (!string.IsNullOrEmpty(body2))
            {
                builder.Append("<text id='3'>")
                    .Append(body2)
                    .Append("</text>");
            }

            builder.Append("</binding>")
                .Append("</visual>")
                .Append("</toast>");

            var toastDom = new Windows.Data.Xml.Dom.XmlDocument();
            toastDom.LoadXml(builder.ToString());
            var toast = new ToastNotification(toastDom);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


    }
}
