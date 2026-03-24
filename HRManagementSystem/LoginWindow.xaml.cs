using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using HRManagementSystem.Views.Admin;
using HRManagementSystem.Views.Employee;
using HRManagementSystem.Views.Manager;
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
using System.Windows.Shapes;

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

                if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    MainEmployee mainEmployee = new MainEmployee();
                    mainEmployee.Show();
                }
                else if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                {
                    MainManager mainManager = new MainManager();
                    mainManager.Show();
                }
                else
                {
                    MainAdmin mainAdmin = new MainAdmin();
                    mainAdmin.Show();
                }

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
