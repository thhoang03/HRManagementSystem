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
using System.Net.Mail;

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
            dpDOB.SelectedDate = null;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtAddress.Text = string.Empty;
            dpHireDate.SelectedDate = null;
            cbDepartments.SelectedValue = null;
            cbPositions.SelectedValue = null;
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
            FillComboDepartments();
            FillComboPositons();
            FillFilterDepartments();
            FillFilterPositions();
            cbFilterStatus.SelectedIndex = 0;
            cbFilterGender.SelectedIndex = 0;
            ApplyFilters();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtName.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetEmployeeInput(out EmployeeInput input))
            {
                return;
            }

            EmployeeModel emp = new EmployeeModel();
            emp.FullName = input.FullName;
            emp.Gender = input.Gender;
            emp.DoB = input.DoB;
            emp.Email = input.Email;
            emp.Phone = input.Phone;
            emp.Address = input.Address;
            emp.HireDate = input.HireDate;
            emp.DepartmentId = input.DepartmentId;
            emp.PositionId = input.PositionId;

            _empBLL.Add(emp);
            ApplyFilters();
            Clear();
        }

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var emp = dgEmployees.SelectedValue as EmployeeModel;
            if (emp != null)
            {
                txtName.Text = emp.FullName;
                rbMale.IsChecked = string.Equals(emp.Gender, "Male", StringComparison.OrdinalIgnoreCase);
                rbFemale.IsChecked = string.Equals(emp.Gender, "Female", StringComparison.OrdinalIgnoreCase);
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
            var emp = dgEmployees.SelectedItem as EmployeeModel;
            if (emp != null)
            {
                if (!TryGetEmployeeInput(out EmployeeInput input))
                {
                    return;
                }

                emp.FullName = input.FullName;
                emp.Gender = input.Gender;
                emp.DoB = input.DoB;
                emp.Email = input.Email;
                emp.Phone = input.Phone;
                emp.Address = input.Address;
                emp.HireDate = input.HireDate;
                emp.DepartmentId = input.DepartmentId;
                emp.PositionId = input.PositionId;

                _empBLL.Update(emp);
                ApplyFilters();
                Clear();
            }
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
                    Clear();
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
                    Clear();
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

        private bool TryGetEmployeeInput(out EmployeeInput input)
        {
            input = new EmployeeInput();

            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Employee name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            if (cbDepartments.SelectedValue == null || !int.TryParse(cbDepartments.SelectedValue.ToString(), out int deptId))
            {
                MessageBox.Show("Please select a valid department.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbDepartments.Focus();
                return false;
            }

            if (cbPositions.SelectedValue == null || !int.TryParse(cbPositions.SelectedValue.ToString(), out int posId))
            {
                MessageBox.Show("Please select a valid position.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbPositions.Focus();
                return false;
            }

            DateOnly? dob = null;
            if (dpDOB.SelectedDate.HasValue)
            {
                DateTime dobDate = dpDOB.SelectedDate.Value.Date;
                if (dobDate > DateTime.Today)
                {
                    MessageBox.Show("Date of birth cannot be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpDOB.Focus();
                    return false;
                }
                dob = DateOnly.FromDateTime(dobDate);
            }

            DateOnly? hireDate = null;
            if (dpHireDate.SelectedDate.HasValue)
            {
                DateTime hireDateValue = dpHireDate.SelectedDate.Value.Date;
                if (hireDateValue > DateTime.Today)
                {
                    MessageBox.Show("Hire date cannot be in the future.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpHireDate.Focus();
                    return false;
                }

                if (dpDOB.SelectedDate.HasValue && hireDateValue < dpDOB.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Hire date cannot be earlier than date of birth.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpHireDate.Focus();
                    return false;
                }
                hireDate = DateOnly.FromDateTime(hireDateValue);
            }

            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!MailAddress.TryCreate(email, out _))
                {
                    MessageBox.Show("Email format is invalid.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            string phone = txtPhone.Text.Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                bool isPhoneValid = phone.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ');
                if (!isPhoneValid)
                {
                    MessageBox.Show("Phone number contains invalid characters.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhone.Focus();
                    return false;
                }
            }

            input = new EmployeeInput
            {
                FullName = name,
                Gender = (rbMale.IsChecked == true) ? "Male" : "Female",
                DoB = dob,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                HireDate = hireDate,
                DepartmentId = deptId,
                PositionId = posId
            };
            return true;
        }

        private sealed class EmployeeInput
        {
            public string FullName { get; set; } = string.Empty;
            public string? Gender { get; set; }
            public DateOnly? DoB { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public DateOnly? HireDate { get; set; }
            public int DepartmentId { get; set; }
            public int PositionId { get; set; }
        }
    }
}



