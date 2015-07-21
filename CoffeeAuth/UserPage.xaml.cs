using CoffeeAuth.Models;
using SQLitePCL;
using System;
using System.Text;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Maker.RemoteWiring;

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
            m_user = DrinkerDatabase.Instance.GetUser(badgeCIN);
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

                m_user.NumLogins++;
                DrinkerDatabase.Instance.UpdateUser(m_user);
            }
        }

        DispatcherTimer timer;
        ContentDialog countdown_dialog;


        private void getCoffeeButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Espresso Shot", "$1 debited from your account", "thank you for shopping!");
            m_user.Balance--;
            m_user.NumShots++;
            userBalance.Text = m_user.Balance.ToString();
            DrinkerDatabase.Instance.UpdateUser(m_user);

            Countdown();
            numTicks = 30;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
            // Turn on grinder
#if !HARDWARE
            App.arduino.digitalWrite(13, PinState.HIGH);
#endif

        }

        int numTicks;
        TextBlock countdown;

        private void Timer_Tick(object sender, object e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            numTicks--;
            if (numTicks == 0)
            {
            // Turn off grinder
#if !HARDWARE
                App.arduino.digitalWrite(13, PinState.LOW);
#endif
                Frame.Navigate(typeof(MainPage));
                countdown_dialog.Hide();
                timer.Stop();
            }
            else
            {
                //update timer UI 
                countdown.Text = numTicks.ToString();
                if (numTicks <= 15)
                    countdown.Foreground = new SolidColorBrush(Colors.Orange);
                if (numTicks <= 5)
                    countdown.Foreground = new SolidColorBrush(Colors.Red);
            }
        }


        private async void Countdown()
        {
            TextBlock text = new TextBlock();
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.Text = "You have 30 seconds to grind your coffee";

            countdown = new TextBlock();
            countdown.Text = "30";
            countdown.FontSize = 48;
            countdown.HorizontalAlignment = HorizontalAlignment.Center;
            countdown.Padding = new Thickness(0, 10, 0, 0);

            StackPanel stack = new StackPanel();
            stack.Margin = new Thickness(0, 40, 0, 0);
            stack.Children.Add(text);
            stack.Children.Add(countdown);

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
            m_user.NumBags++;
            userBalance.Text = m_user.Balance.ToString();
            DrinkerDatabase.Instance.UpdateUser(m_user);
        }

        private void milkButton_Click(object sender, RoutedEventArgs e)
        {
            showToast("Milk Jug Deposited", "$6 credited to your account.", "Everyone appreciates your efforts");
            m_user.Balance += 6;
            m_user.NumMilks++;
            userBalance.Text = m_user.Balance.ToString();
            DrinkerDatabase.Instance.UpdateUser(m_user);
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
                DrinkerDatabase.Instance.UpdateUser(m_user);

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
                DrinkerDatabase.Instance.UpdateUser(m_user);

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
