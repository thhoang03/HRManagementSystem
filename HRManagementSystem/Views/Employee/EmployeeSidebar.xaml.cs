using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Employee
{
    /// <summary>
    /// Interaction logic for EmployeeSidebar.xaml
    /// </summary>
    public partial class EmployeeSidebar : UserControl
    {
        public EmployeeSidebar()
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

        private void btnAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainEmployee mainEmployee)
            {
                mainEmployee.MainFrame.Navigate(new CheckInOutView());
            }
        }

        private void btnLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainEmployee mainEmployee)
            {
                mainEmployee.MainFrame.Navigate(new MyLeaveRequestsView());
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties.Remove("CurrentUser");

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            if (Window.GetWindow(this) is MainEmployee mainEmployee)
            {
                mainEmployee.Close();
            }
        }
    }
}

