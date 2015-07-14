using CoffeeAuth.Models;
using Microsoft.Maker.RemoteWiring;
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
using Windows.UI;
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

        ContentDialog countdown_dialog;

        private async void getCoffeeButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Espresso Shot", "$1 debited from your account", "thank you for shopping!");
            m_user.Balance--;
            userBalance.Text = m_user.Balance.ToString();
            UpdateUserBalance(m_user);

            // Turn on grinder
            Countdown();
            App.arduino.digitalWrite(13, PinState.HIGH);

            await Task.Delay(30000).ContinueWith(_ =>
            {
                App.arduino.digitalWrite(13, PinState.LOW);
            });

            Frame.Navigate(typeof(MainPage));
            countdown_dialog.Hide();
        }

        private async void Countdown()
        {
            TextBlock text = new TextBlock();
            text.Text = "You have 30 seconds to grind your coffee";

            StackPanel stack = new StackPanel();
            stack.Margin = new Thickness(0, 40, 0, 0);
            stack.Children.Add(text);

            countdown_dialog = new ContentDialog();
            countdown_dialog.Content = stack;
            SolidColorBrush color = new SolidColorBrush(Colors.Black);
            countdown_dialog.Background = color;
            await countdown_dialog.ShowAsync();
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
            Frame.Navigate(typeof(MainPage));
        }

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


        

        ContentDialog m_settleDialog;
        TextBox m_input;

        private async void settleButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock title = new TextBlock();
            title.Text = "Enter a positive or negative value";

            m_input = new TextBox();
            m_input.PlaceholderText = "Amount";
            m_input.InputScope = new InputScope
            {
                Names =
                {
                    new InputScopeName(InputScopeNameValue.Digits)
                }
            };

            Button debit = new Button();
            debit.Content = "Debit";
            debit.Click += Debit_Click;
            debit.HorizontalAlignment = HorizontalAlignment.Right;

            Button credit = new Button();
            credit.Content = "Credit";
            credit.Click += Credit_Click; ;
            credit.HorizontalAlignment = HorizontalAlignment.Right;

            Button cancel = new Button();
            cancel.Content = "Cancel";
            cancel.Click += Cancel_Click;
            cancel.HorizontalAlignment = HorizontalAlignment.Right;

            StackPanel buttonStack = new StackPanel();
            buttonStack.Orientation = Orientation.Horizontal;
            buttonStack.HorizontalAlignment = HorizontalAlignment.Right;
            buttonStack.Children.Add(cancel);
            buttonStack.Children.Add(debit);
            buttonStack.Children.Add(credit);

            StackPanel stack = new StackPanel();
            stack.Margin = new Thickness(0, 40, 0, 0);
            stack.Children.Add(title);
            stack.Children.Add(m_input);
            stack.Children.Add(buttonStack);

            m_settleDialog = new ContentDialog();
            m_settleDialog.Content = stack;
            m_settleDialog.Background = new SolidColorBrush(Colors.Black);

            await m_settleDialog.ShowAsync();
        }

        private void Credit_Click(object sender, RoutedEventArgs e)
        {
            string val = m_input.Text;
            try
            {
                int num = Convert.ToInt32(val);

                string body = "$" + num + " credited to your account.";
                showToast("Settled Up", body, "Everyone appreciates your efforts");

                m_user.Balance += num;
                userBalance.Text = m_user.Balance.ToString();
                UpdateUserBalance(m_user);

                m_settleDialog.Hide();
            }
            catch
            {
                // todo show error
            }
        }

        

        private void Debit_Click(object sender, RoutedEventArgs e)
        {
            string val = m_input.Text;
            try
            {
                int num = Convert.ToInt32(val);

                string body = "$" + num + " debited from your account.";
                showToast("Settled Up", body, "Everyone appreciates your efforts");

                m_user.Balance -= num;
                userBalance.Text = m_user.Balance.ToString();
                UpdateUserBalance(m_user);

                m_settleDialog.Hide();
            }
            catch
            {
                // todo show error
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            m_settleDialog.Hide();
        }
    }
}
