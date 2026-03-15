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
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Page
    {
        private readonly EmployeeBLL _empBLL = new();
        private readonly DepartmentBLL _deptBLL = new();
        private readonly PositionBLL _posBLL = new();
        private readonly ContractBLL _contBLL = new();
        private readonly UserBLL _userBLL = new();

        public DashboardView()
        {
            InitializeComponent();
        }

        private void frmDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStats();
            LoadRecentEmployees();
            LoadRecentLogins();
        }

        private void LoadStats()
        {
            var employees = _empBLL.GetAll();
            var departments = _deptBLL.GetAll();
            var positions = _posBLL.GetAll();
            var users = _userBLL.GetAll();
            var contracts = _contBLL.GetAll();

            txtEmployeesCount.Text = employees.Count.ToString();
            txtDepartmentsCount.Text = departments.Count.ToString();
            txtPositionsCount.Text = positions.Count.ToString();
            txtUsersCount.Text = users.Count.ToString();
            txtContractsCount.Text = contracts.Count.ToString();

            int activeContracts = contracts.Count(c =>
                !string.IsNullOrWhiteSpace(c.Status)
                && c.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            txtActiveContractsCount.Text = activeContracts.ToString();
        }

        private void LoadRecentEmployees()
        {
            var employees = _empBLL.GetAll()
                .OrderByDescending(e => e.HireDate ?? DateOnly.MinValue)
                .ThenByDescending(e => e.EmployeeId)
                .Take(8)
                .ToList();

            dgRecentEmployees.ItemsSource = null;
            dgRecentEmployees.ItemsSource = employees;
        }

        private void LoadRecentLogins()
        {
            var users = _userBLL.GetAll()
                .OrderByDescending(u => u.LastLogin ?? DateTime.MinValue)
                .ThenByDescending(u => u.UserId)
                .Take(8)
                .ToList();

            dgRecentLogins.ItemsSource = null;
            dgRecentLogins.ItemsSource = users;
        }
    }
}
