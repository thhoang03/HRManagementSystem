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
            string name = txtDeptName.Text.Trim();
            string des = txtDescription.Text.Trim();
            if (!ValidateDepartmentInput(name))
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
                string deptName = txtDeptName.Text.Trim();
                if (!ValidateDepartmentInput(deptName))
                {
                    return;
                }

                dept.DepartmentName = deptName;
                dept.Description = txtDescription.Text.Trim();

                _deptBLL.Update(dept);
                FillDataGridDepartments();
                Clear();
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

        private bool ValidateDepartmentInput(string departmentName)
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
            _deptBLL.Update(dept);
            FillDataGridDepartments();
        }
    }
}
