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
    /// Interaction logic for EmployeesView.xaml
    /// </summary>
    public partial class EmployeesView : Page
    {
        private DepartmentBLL _deptBLL = new();
        private PositionBLL _posBLL = new();
        private EmployeeBLL _empBLL = new();
        public EmployeesView()
        {
            InitializeComponent();
        }

        private void Clear()
        {
            txtName.Text = string.Empty;
            rbMale.IsChecked = true;
            dpDOB = null;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtAddress.Text = string.Empty;
            dpHireDate = null;
            cbDepartments = null;
            cbPositions = null;
        }

        private void FillComboDepartments()
        {
            cbDepartments.ItemsSource = null;
            cbDepartments.ItemsSource = _deptBLL.GetAll();
            cbDepartments.SelectedValuePath = "DepartmentId";
            cbDepartments.DisplayMemberPath = "DepartmentName";
        }

        private void FillComboPositons()
        {
            cbPositions.ItemsSource = null;
            cbPositions.ItemsSource = _posBLL.GetAll();
            cbPositions.SelectedValuePath = "PositionId";
            cbPositions.DisplayMemberPath = "PositionName";
        }

        private void FillDataGridEmployees()
        {
            dgEmployees.ItemsSource = null;
            dgEmployees.ItemsSource = _empBLL.GetAll();
        }

        private void frmEmployees_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboDepartments();
            FillComboPositons();
            FillDataGridEmployees();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtName.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string gender = (rbMale.IsChecked == true) ? "Male" : "Female";
            DateOnly? dob = null;
            if (dpDOB.SelectedDate.HasValue)
            {
                dob = DateOnly.FromDateTime(dpDOB.SelectedDate.Value);
            }
            string mail = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();
            DateOnly? hireDate = null;
            if (dpHireDate.SelectedDate.HasValue)
            {
                hireDate = DateOnly.FromDateTime(dpHireDate.SelectedDate.Value);
            }
            string? deptId_str = cbDepartments.SelectedValue.ToString();
            string? posId_str = cbPositions.SelectedValue.ToString();

            //validate

            int.TryParse(deptId_str, out int deptId);
            int.TryParse(posId_str, out int posId);
            //

            //
            Employee emp = new Employee();
            emp.FullName = name;
            emp.Gender = gender;
            emp.DoB = dob;
            emp.Email = mail;
            emp.Phone = phone;
            emp.Address = address;
            emp.HireDate = hireDate;
            emp.DepartmentId = deptId;
            emp.PositionId = posId;

            _empBLL.Add(emp);
            FillDataGridEmployees();
            Clear();
        }

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var emp = dgEmployees.SelectedValue as Employee;
            if (emp != null)
            {
                txtName.Text = emp.FullName;
                rbMale.IsChecked = emp.Gender.Equals("Male") ? true : false;
                rbFemale.IsChecked = emp.Gender.Equals("Female") ? true : false;
                dpDOB.SelectedDate = emp.DoB?.ToDateTime(TimeOnly.MinValue);
                txtEmail.Text = emp.Email;
                txtPhone.Text = emp.Phone;
                txtAddress.Text = emp.Address;
                dpHireDate.SelectedDate = emp.HireDate?.ToDateTime(TimeOnly.MinValue);
                cbDepartments.SelectedValue = emp.DepartmentId;
                cbPositions.SelectedValue = emp.PositionId;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as Employee;
            if (emp != null)
            {
                string name = txtName.Text.Trim();
                string gender = (rbMale.IsChecked == true) ? "Male" : "Female";
                DateOnly? dob = null;
                if (dpDOB.SelectedDate.HasValue)
                {
                    dob = DateOnly.FromDateTime(dpDOB.SelectedDate.Value);
                }
                string mail = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string address = txtAddress.Text.Trim();
                DateOnly? hireDate = null;
                if (dpHireDate.SelectedDate.HasValue)
                {
                    hireDate = DateOnly.FromDateTime(dpHireDate.SelectedDate.Value);
                }
                string? deptId_str = cbDepartments.SelectedValue.ToString();
                string? posId_str = cbPositions.SelectedValue.ToString();

                //validate

                int.TryParse(deptId_str, out int deptId);
                int.TryParse(posId_str, out int posId);
                //

                //
                emp.FullName = name;
                emp.Gender = gender;
                emp.DoB = dob;
                emp.Email = mail;
                emp.Phone = phone;
                emp.Address = address;
                emp.HireDate = hireDate;
                emp.DepartmentId = deptId;
                emp.PositionId = posId;

                _empBLL.Update(emp);
                FillDataGridEmployees();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as Employee;
            if (emp != null)
            {
                if(MessageBox.Show("Do you really want to delete this employee?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning)==MessageBoxResult.Yes)
                {
                    _empBLL.Delete(emp);
                    FillDataGridEmployees();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtSearch.Text.Trim();
            dgEmployees.ItemsSource = _empBLL.Search(name);
        }
    }
}
