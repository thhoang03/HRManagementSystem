using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using HRManagementSystem.Views.Admin;
using HRManagementSystem.Views.Employee;
// HR views may be present; ensure namespace reference is valid when HR files exist
using HRManagementSystem.Views.Manager;
using System;
using System.Windows;

namespace HRManagementSystem
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly UserBLL _userBLL = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User? user = _userBLL.Authenticate(username, password);
            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                _userBLL.Update(user);

                Application.Current.Properties["CurrentUser"] = user;
                string role = user.Role?.Trim() ?? string.Empty;

                Window? mainWindow = role.ToLowerInvariant() switch
                {
                    "employee" => new MainEmployee(),
                    "manager" => new MainManager(),
                    "admin" => new MainAdmin(),
                    "hr" => new HRManagementSystem.Views.HR.MainHR(),
                    _ => null
                };

                if (mainWindow is null)
                {
                    MessageBox.Show("Vai trò người dùng không hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Password = string.Empty;
                txtPassword.Focus();
            }
        }
    }
}
