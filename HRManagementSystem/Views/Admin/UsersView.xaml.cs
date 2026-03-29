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
            cbStatus.SelectedIndex = 0;
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
                SetStatusSelection(user.Status);
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
            user.Status = GetSelectedStatus();
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
                user.Status = GetSelectedStatus();
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
                if (MessageBox.Show("Do you really want to deactivate this user?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    user.Status = "Deactive";
                    _userBLL.Update(user);
                    FillDataGridUsers();
                    Clear();
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            var user = dgUsers.SelectedItem as User;
            if (user != null)
            {
                if (MessageBox.Show("Do you really want to active this user?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    user.Status = "Active";
                    _userBLL.Update(user);
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

        private string GetSelectedStatus()
        {
            if (cbStatus.SelectedItem is ComboBoxItem item
                && item.Content is string status
                && !string.IsNullOrWhiteSpace(status))
            {
                return status;
            }

            return "Active";
        }

        private void SetStatusSelection(string? status)
        {
            string normalized = string.IsNullOrWhiteSpace(status) ? "Active" : status.Trim();
            foreach (var item in cbStatus.Items)
            {
                if (item is ComboBoxItem comboItem
                    && comboItem.Content is string content
                    && string.Equals(content, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    cbStatus.SelectedItem = comboItem;
                    return;
                }
            }

            cbStatus.SelectedIndex = 0;
        }
    }
}
