using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Manager
{
    /// <summary>
    /// Interaction logic for ManagerSidebar.xaml
    /// </summary>
    public partial class ManagerSidebar : UserControl
    {
        public ManagerSidebar()
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

        private void btnLeaveApproval_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainManager mainManager)
            {
                mainManager.MainFrame.Navigate(new ApproveLeaveView());
            }
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainManager mainManager)
            {
                mainManager.MainFrame.Navigate(new ReportsView());
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties.Remove("CurrentUser");

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            if (Window.GetWindow(this) is MainManager mainManager)
            {
                mainManager.Close();
            }
        }
    }
}

