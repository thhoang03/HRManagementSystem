using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        private const double ExpandedWidth = 240;
        private const double CollapsedWidth = 70;
        private bool _isCollapsed;

        public Sidebar()
        {
            InitializeComponent();
            if (Application.Current.Properties.Contains("CurrentUser"))
            {
                var user = Application.Current.Properties["CurrentUser"] as User;
                txtUserName.Text = user.Username;
                txtUserRole.Text = user.Role;
            }

            SetCollapsed(false);
        }

        private void btnEmployees_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new EmployeesView());
            }
        }

        private void btnDepartments_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new DepartmentsView());
            }
        }

        private void btnPositions_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new PositionsView());
            }
        }

        private void btnContracts_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new ContractsView());
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new UsersView());
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new DashboardView());
            }
        }

        private void btnAttendance_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new AttendanceView());
            }
        }

        private void btnPayroll_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new PayrollView());
            }
        }

        private void btnLeaveRequests_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new LeaveRequestsView());
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;
            if (mainAdmin != null)
            {
                mainAdmin.MainFrame.Navigate(new SettingsView());
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var mainAdmin = Window.GetWindow(this) as MainAdmin;

            Application.Current.Properties.Remove("CurrentUser");

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            if (mainAdmin != null)
            {
                mainAdmin.Close();
            }
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            SetCollapsed(!_isCollapsed);
        }

        private void SetCollapsed(bool collapsed)
        {
            _isCollapsed = collapsed;
            Width = collapsed ? CollapsedWidth : ExpandedWidth;
            btnToggle.Content = collapsed ? ">>" : "<<";
            btnToggle.ToolTip = collapsed ? "Expand sidebar" : "Collapse sidebar";

            var visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
            txtAppName.Visibility = visibility;
            txtAppSubName.Visibility = visibility;
            bdUserInfo.Visibility = visibility;

            foreach (var button in GetMenuButtons())
            {
                if (button.Tag is string fullText && !string.IsNullOrWhiteSpace(fullText))
                {
                    button.Content = collapsed ? fullText.Substring(0, 1) : fullText;
                }

                button.HorizontalContentAlignment = collapsed
                    ? HorizontalAlignment.Center
                    : HorizontalAlignment.Left;
            }
        }

        private IEnumerable<Button> GetMenuButtons()
        {
            return new[]
            {
                btnDashboard,
                btnUser,
                btnDepartments,
                btnPositions,
                btnEmployees,
                btnContracts,
                btnAttendance,
                btnPayroll,
                btnLeaveRequests,
                btnSettings,
                btnReports,
                btnLogout
            };
        }
    }
}
