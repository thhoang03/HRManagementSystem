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
        public Sidebar()
        {
            InitializeComponent();
            if (Application.Current.Properties.Contains("CurrentUser"))
        {
            var user = Application.Current.Properties["CurrentUser"] as User;
            txtUserName.Text =  user.Username;
            txtUserRole.Text =  user.Role;

        }
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
    }
}
