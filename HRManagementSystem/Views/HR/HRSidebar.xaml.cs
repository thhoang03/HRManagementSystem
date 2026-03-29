using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.HR
{
    /// <summary>
    /// Interaction logic for HRSidebar.xaml
    /// </summary>
    public partial class HRSidebar : UserControl
    {
        public HRSidebar()
        {
            InitializeComponent();

            if (Application.Current.Properties.Contains("CurrentUser"))
            {
                var user = Application.Current.Properties["CurrentUser"] as User;
                if (user != null)
                {
                    txtUserName.Text = user.Username;
                    txtUserRole.Text = user.Role;
                }
            }
        }

        private void btnHRDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainHR mainHr)
            {
                mainHr.MainFrame.Navigate(new Employee());
            }
        }

        private void btnContracts_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainHR mainHr)
            {
                mainHr.MainFrame.Navigate(new ContractsView());
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties.Remove("CurrentUser");

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            if (Window.GetWindow(this) is MainHR mainHr)
            {
                mainHr.Close();
            }
        }
    }
}
