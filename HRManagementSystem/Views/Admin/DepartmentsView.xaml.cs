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
    /// Interaction logic for DepartmentsView.xaml
    /// </summary>
    public partial class DepartmentsView : Page
    {
        private DepartmentBLL _deptBLL = new();
        public DepartmentsView()
        {
            InitializeComponent();
        }

        private void FillDataGridDepartments()
        {
            dgDepartments.ItemsSource = null;
            dgDepartments.ItemsSource = _deptBLL.GetAll();
        }

        private void Clear()
        {
            txtDeptName.Text = string.Empty;
            txtDescription.Text = string.Empty;
        }

        private void frmDepartments_Loaded(object sender, RoutedEventArgs e)
        {
            FillDataGridDepartments();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtDeptName.Text.Trim();
                string des = txtDescription.Text.Trim();
                if (!ValidateDepartmentInput(name, null))
                {
                    return;
                }

                Department dept = new Department();
                dept.DepartmentName = name;
                dept.Description = des;
                dept.Status = "Active";
                _deptBLL.Add(dept);
                FillDataGridDepartments();
                Clear();
            }
            catch (Exception ex)
            {
                ShowDepartmentSaveError(ex);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtDeptName.Focus();
        }

        private void dgDepartments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept != null)
            {
                txtDeptName.Text = dept.DepartmentName;
                txtDescription.Text = dept.Description;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept != null)
            {
                try
                {
                    string deptName = txtDeptName.Text.Trim();
                    if (!ValidateDepartmentInput(deptName, dept.DepartmentId))
                    {
                        return;
                    }

                    dept.DepartmentName = deptName;
                    dept.Description = txtDescription.Text.Trim();

                    _deptBLL.Update(dept);
                    FillDataGridDepartments();
                    Clear();
                }
                catch (Exception ex)
                {
                    ShowDepartmentSaveError(ex);
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Active");
        }

        private void btnDeactive_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Deactive");
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtSearch.Text.Trim();
            dgDepartments.ItemsSource = null;
            dgDepartments.ItemsSource = _deptBLL.Search(name);
        }

        private bool ValidateDepartmentInput(string departmentName, int? excludeDepartmentId)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                MessageBox.Show("Department name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeptName.Focus();
                return false;
            }

            if (departmentName.Length > 100)
            {
                MessageBox.Show("Department name must be 100 characters or fewer.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeptName.Focus();
                return false;
            }

            if (!_deptBLL.IsDepartmentNameUnique(departmentName, excludeDepartmentId))
            {
                MessageBox.Show("Department name already exists. Please choose a different name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeptName.Focus();
                return false;
            }

            return true;
        }

        private void UpdateSelectedStatus(string status)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept == null)
            {
                MessageBox.Show("Please select a department first.", "Department", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dept.Status = status;
            try
            {
                _deptBLL.Update(dept);
                FillDataGridDepartments();
            }
            catch (Exception ex)
            {
                ShowDepartmentSaveError(ex);
            }
        }

        private void ShowDepartmentSaveError(Exception ex)
        {
            string message = ex.Message ?? string.Empty;
            if (IsUniqueConstraintViolation(message))
            {
                MessageBox.Show("Department name already exists. Please choose a different name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDeptName.Focus();
                return;
            }

            MessageBox.Show($"Cannot save department. {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static bool IsUniqueConstraintViolation(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            return errorMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
