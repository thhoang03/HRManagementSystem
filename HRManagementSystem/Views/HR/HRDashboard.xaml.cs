using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System;

namespace HRManagementSystem.Views.HR
{
    /// <summary>
    /// Interaction logic for HRDashboard.xaml
    /// </summary>
    public partial class HRDashboard : UserControl
    {
        private readonly EmployeeBLL _empBLL = new();
        private readonly DepartmentBLL _depBLL = new();
        private readonly PositionBLL _posBLL = new();

        public HRDashboard()
        {
            InitializeComponent();
            Loaded += HRDashboard_Loaded;
        }

        private void HRDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDepartmentsAndPositions();
            LoadEmployees();
        }

        private void LoadDepartmentsAndPositions()
        {
            cbDepartment.ItemsSource = _depBLL.GetAll();
            cbPosition.ItemsSource = _posBLL.GetAll();
        }

        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = null;
            var all = _empBLL.GetAll();
            var visibles = all.Where(e => string.IsNullOrWhiteSpace(e.Status) || !e.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)).ToList();
            dgEmployees.ItemsSource = visibles;
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetEmployeeInput(out var input))
            {
                return;
            }

            var emp = new HRManagementSystem.Models.Employee
            {
                FullName = input.FullName,
                Email = input.Email,
                Phone = input.Phone,
                DepartmentId = input.DepartmentId,
                PositionId = input.PositionId,
                Status = "Active",
                HireDate = input.HireDate
            };

            try
            {
                _empBLL.Add(emp);
                MessageBox.Show("Employee added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // refresh employee list in dashboard so newly added employee appears
                LoadEmployees();

                // navigate to Contracts so HR can add contract for this employee
                if (Window.GetWindow(this) is MainHR mainHr)
                {
                    var contractsPage = new ContractsView();
                    mainHr.MainFrame.Navigate(contractsPage);
                    // ensure the new employee is loaded and selected
                    contractsPage.SelectEmployeeById(emp.EmployeeId);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGoContracts_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainHR mainHr)
            {
                mainHr.MainFrame.Navigate(new ContractsView());
            }
        }

        private bool TryGetEmployeeInput(out EmployeeInput input)
        {
            input = new EmployeeInput();

            string fullName = txtFullName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Full name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            // Email validation: must contain '@' if provided
            string? email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!email.Contains("@"))
                {
                    MessageBox.Show("Email must contain '@'.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            // Phone validation: required and must be exactly 10 digits
            string? phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Phone is required and must be exactly 10 digits.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            if (phone.Length != 10 || !phone.All(char.IsDigit))
            {
                MessageBox.Show("Phone must be exactly 10 digits and contain only numbers.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            int? depId = null;
            if (cbDepartment.SelectedValue != null && int.TryParse(cbDepartment.SelectedValue.ToString(), out int d)) depId = d;
            else
            {
                MessageBox.Show("Please select a Department.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbDepartment.Focus();
                return false;
            }

            int? posId = null;
            if (cbPosition.SelectedValue != null && int.TryParse(cbPosition.SelectedValue.ToString(), out int p)) posId = p;
            else
            {
                MessageBox.Show("Please select a Position.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbPosition.Focus();
                return false;
            }

            input = new EmployeeInput
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                DepartmentId = depId,
                PositionId = posId,
                HireDate = null
            };

            return true;
        }

        private sealed class EmployeeInput
        {
            public string FullName { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public int? DepartmentId { get; set; }
            public int? PositionId { get; set; }
            public DateOnly? HireDate { get; set; }
        }

        private void txtPhone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // allow only digits
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void txtPhone_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var pasteText = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
                // keep only digits and ensure not longer than 10
                if (string.IsNullOrEmpty(pasteText) || !pasteText.All(char.IsDigit) || pasteText.Length > 10)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void btnDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null)
            {
                MessageBox.Show("Please select an employee to delete.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Do you really want to deactivate employee '{emp.FullName}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    emp.Status = "Inactive";
                    _empBLL.Update(emp);
                    LoadEmployees();
                    MessageBox.Show("Employee marked as Inactive.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
