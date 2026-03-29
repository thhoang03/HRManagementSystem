using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using EmployeeModel = HRManagementSystem.Models.Employee;
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
    /// Interaction logic for EmployeesView.xaml
    /// </summary>
    public partial class EmployeesView : Page
    {
        private readonly DepartmentBLL _deptBLL = new();
        private readonly PositionBLL _posBLL = new();
        private readonly EmployeeBLL _empBLL = new();
        public EmployeesView()
        {
            InitializeComponent();
        }

        private void FillFilterDepartments()
        {
            var departments = _deptBLL.GetAll();
            cbFilterDepartment.ItemsSource = null;
            cbFilterDepartment.ItemsSource = departments;
            cbFilterDepartment.DisplayMemberPath = "DepartmentName";
            cbFilterDepartment.SelectedValuePath = "DepartmentId";
            cbFilterDepartment.SelectedValue = null;
            cbFilterDepartment.Text = "All Departments";
        }

        private void FillFilterPositions()
        {
            var positions = _posBLL.GetAll();
            cbFilterPosition.ItemsSource = null;
            cbFilterPosition.ItemsSource = positions;
            cbFilterPosition.DisplayMemberPath = "PositionName";
            cbFilterPosition.SelectedValuePath = "PositionId";
            cbFilterPosition.SelectedValue = null;
            cbFilterPosition.Text = "All Positions";
        }

        private void ApplyFilters()
        {
            string keyword = txtSearch.Text.Trim();
            var filtered = _empBLL.GetAll().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(e =>
                    (!string.IsNullOrWhiteSpace(e.FullName) && e.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(e.Email) && e.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(e.Phone) && e.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (cbFilterDepartment.SelectedValue != null && int.TryParse(cbFilterDepartment.SelectedValue.ToString(), out int deptId))
            {
                filtered = filtered.Where(e => e.DepartmentId == deptId);
            }

            if (cbFilterPosition.SelectedValue != null && int.TryParse(cbFilterPosition.SelectedValue.ToString(), out int posId))
            {
                filtered = filtered.Where(e => e.PositionId == posId);
            }

            string status = (cbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(e => string.Equals(e.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            string gender = (cbFilterGender.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            if (!string.Equals(gender, "All", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(e => string.Equals(e.Gender, gender, StringComparison.OrdinalIgnoreCase));
            }

            dgEmployees.ItemsSource = null;
            dgEmployees.ItemsSource = filtered.ToList();
        }

        private void frmEmployees_Loaded(object sender, RoutedEventArgs e)
        {
            FillFilterDepartments();
            FillFilterPositions();
            cbFilterStatus.SelectedIndex = 0;
            cbFilterGender.SelectedIndex = 0;
            ApplyFilters();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as EmployeeModel;
            if (emp != null)
            {
                if(MessageBox.Show("Do you really want to deactivate this employee?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning)==MessageBoxResult.Yes)
                {
                    emp.Status = "Deactive";
                    _empBLL.Update(emp);
                    ApplyFilters();
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as EmployeeModel;
            if (emp != null)
            {
                if (MessageBox.Show("Do you really want to active this employee?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    emp.Status = "Active";
                    _empBLL.Update(emp);
                    ApplyFilters();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void cbFilterPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void cbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void cbFilterGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            cbFilterDepartment.SelectedValue = null;
            cbFilterPosition.SelectedValue = null;
            cbFilterStatus.SelectedIndex = 0;
            cbFilterGender.SelectedIndex = 0;
            ApplyFilters();
        }
    }
}



