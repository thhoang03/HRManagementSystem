using HRManagementSystem.BLL;
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
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : Page
    {
        private readonly UserBLL _userBLL = new();
        private readonly EmployeeBLL _empBLL = new();

        public UsersView()
        {
            InitializeComponent();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        private void FillDataGridUsers()
        {
            dgUsers.ItemsSource = null;
            dgUsers.ItemsSource = _userBLL.GetAll();
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            txtUsername.Text = string.Empty;
            pwdPassword.Password = string.Empty;
            txtRole.Text = string.Empty;
            txtLastLogin.Text = string.Empty;
        }

        private void frmUsers_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            FillDataGridUsers();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtUsername.Focus();
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = dgUsers.SelectedItem as User;
            if (user != null)
            {
                cbEmployees.SelectedValue = user.EmployeeId;
                txtUsername.Text = user.Username;
                txtRole.Text = user.Role ?? string.Empty;
                txtLastLogin.Text = user.LastLogin?.ToString("g") ?? string.Empty;
                pwdPassword.Password = string.Empty;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = pwdPassword.Password.Trim();
            string role = txtRole.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            int? employeeId = null;
            if (cbEmployees.SelectedValue != null && int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId))
            {
                employeeId = empId;
            }

            User user = new();
            user.Username = username;
            user.PasswordHash = password;
            user.Role = string.IsNullOrWhiteSpace(role) ? null : role;
            user.EmployeeId = employeeId;

            _userBLL.Add(user);
            FillDataGridUsers();
            Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var user = dgUsers.SelectedItem as User;
            if (user != null)
            {
                string username = txtUsername.Text.Trim();
                string password = pwdPassword.Password.Trim();
                string role = txtRole.Text.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    return;
                }

                int? employeeId = null;
                if (cbEmployees.SelectedValue != null && int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId))
                {
                    employeeId = empId;
                }

                user.Username = username;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    user.PasswordHash = password;
                }
                user.Role = string.IsNullOrWhiteSpace(role) ? null : role;
                user.EmployeeId = employeeId;

                _userBLL.Update(user);
                FillDataGridUsers();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var user = dgUsers.SelectedItem as User;
            if (user != null)
            {
                if (MessageBox.Show("Do you really want to delete this user?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _userBLL.Delete(user);
                    FillDataGridUsers();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgUsers.ItemsSource = null;
            dgUsers.ItemsSource = _userBLL.Search(keyword);
        }
    }
}
