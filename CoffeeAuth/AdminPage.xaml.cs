using CoffeeAuth.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoffeeAuth
{
    /// <summary>
    /// Administration page for viewing and deleting users 
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        private User currUser;

        public AdminPage()
        {
            this.InitializeComponent();
            UpdateListView();
        }

        private void UpdateListView()
        {
            var users = DrinkerDatabase.Instance.GetAllUsers();
            listView.ItemsSource = users;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                currUser = (User)e.AddedItems[0];
                userName.Text = currUser.Name;
                ToggleUserDisplay(true);
            }
        }

        private void deleteUser_Click(object sender, RoutedEventArgs e)
        {
            DrinkerDatabase.Instance.DeleteUser(currUser);
            ToggleUserDisplay(false);
            UpdateListView();
        }

        private void toggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            currUser.IsAdmin = !currUser.IsAdmin;
            DrinkerDatabase.Instance.UpdateUser(currUser);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void ToggleUserDisplay(bool isVisable)
        {
            if (isVisable)
            {
                userName.Visibility = Visibility.Visible;
                profilePicture.Visibility = Visibility.Visible;
                deleteUser.Visibility = Visibility.Visible;
                toggleAdmin.Content = currUser.IsAdmin ? "Remove Admin" : "Make Admin";
                toggleAdmin.Visibility = Visibility.Visible;
            }
            else
            {
                userName.Visibility = Visibility.Collapsed;
                profilePicture.Visibility = Visibility.Collapsed;
                deleteUser.Visibility = Visibility.Collapsed;
                toggleAdmin.Visibility = Visibility.Collapsed;
            }
        }
    }
}
