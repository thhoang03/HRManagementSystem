using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace HRManagementSystem.Views.HR
{
public class EmployeeInput
{
    public string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateTime? HireDate { get; set; }
}


    public partial class Employee : UserControl
    {
        private readonly EmployeeBLL _empBLL = new();
        private readonly DepartmentBLL _depBLL = new();
        private readonly PositionBLL _posBLL = new();

        public Employee()
        {
            InitializeComponent();
            Loaded += Employee_Loaded;
        }

        private void Employee_Loaded(object sender, RoutedEventArgs e)
        {
            cbDepartment.ItemsSource = _depBLL.GetAll();
            cbPosition.ItemsSource = _posBLL.GetAll();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = _empBLL.GetAll().Where(e => string.IsNullOrWhiteSpace(e.Status) || !e.Status.Equals("Inactive", System.StringComparison.OrdinalIgnoreCase)).ToList();
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
                HireDate = input.HireDate.HasValue ? DateOnly.FromDateTime(input.HireDate.Value) : null
            };

            try
            {
                _empBLL.Add(emp);
                MessageBox.Show("Employee added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadEmployees();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearDetailInputs();
            txtFullName.Focus();
        }

        private void btnDeactivate_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null)
            {
                MessageBox.Show("Please select an employee to deactivate.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Do you really want to deactivate employee '{emp.FullName}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                emp.Status = "Inactive";
                _empBLL.Update(emp);
                LoadEmployees();
            }
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null)
            {
                MessageBox.Show("Please select an employee to activate.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            emp.Status = "Active";
            _empBLL.Update(emp);
            LoadEmployees();
        }

        private void btnUpdateEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null)
            {
                MessageBox.Show("Please select an employee to update.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryGetEmployeeInput(out var input)) return;

            emp.FullName = input.FullName;
            emp.Email = input.Email;
            emp.Phone = input.Phone;
            emp.DepartmentId = input.DepartmentId;
            emp.PositionId = input.PositionId;
            _empBLL.Update(emp);
            LoadEmployees();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var kw = (this.FindName("txtSearchEmployees") as System.Windows.Controls.TextBox)?.Text.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kw)) { LoadEmployees(); return; }
            dgEmployees.ItemsSource = _empBLL.Search(kw).ToList();
        }

        private void ClearDetailInputs()
        {
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPhone.Text = string.Empty;
            cbDepartment.SelectedValue = null;
            cbPosition.SelectedValue = null;
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

            string? email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
            {
                MessageBox.Show("Email must contain '@'.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            string? phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone) || phone.Length != 10 || !phone.All(char.IsDigit))
            {
                MessageBox.Show("Phone must be exactly 10 digits and contain only numbers.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            int? depId = null;
            if (cbDepartment.SelectedValue != null && int.TryParse(cbDepartment.SelectedValue.ToString(), out int d)) depId = d;
            else { MessageBox.Show("Please select a Department.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); cbDepartment.Focus(); return false; }

            int? posId = null;
            if (cbPosition.SelectedValue != null && int.TryParse(cbPosition.SelectedValue.ToString(), out int p)) posId = p;
            else { MessageBox.Show("Please select a Position.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); cbPosition.Focus(); return false; }

            input = new EmployeeInput { FullName = fullName, Email = email, Phone = phone, DepartmentId = depId, PositionId = posId, HireDate = null };
            return true;
        }

        private void btnGoContracts_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainHR mainHr)
            {
                mainHr.MainFrame.Navigate(new ContractsView());
            }
        }

        private void txtSearchEmployees_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = (this.FindName("txtSearchEmployees") as System.Windows.Controls.TextBox)?.Text.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keyword)) { LoadEmployees(); return; }
            dgEmployees.ItemsSource = _empBLL.Search(keyword).Where(x => string.IsNullOrWhiteSpace(x.Status) || !x.Status.Equals("Inactive", System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var emp = dgEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null)
            {
                ClearDetailInputs();
                return;
            }

            // Populate detail fields for viewing/updating
            txtFullName.Text = emp.FullName ?? string.Empty;
            txtEmail.Text = emp.Email ?? string.Empty;
            txtPhone.Text = emp.Phone ?? string.Empty;
            cbDepartment.SelectedValue = emp.DepartmentId;
            cbPosition.SelectedValue = emp.PositionId;

            // If employee is inactive, offer to activate (user can still view/update fields)
            if (!string.IsNullOrWhiteSpace(emp.Status) && emp.Status.Equals("Inactive", System.StringComparison.OrdinalIgnoreCase))
            {
                if (MessageBox.Show($"Nhân viên '{emp.FullName}' hiện Inactive. Bạn có muốn active nhân viên này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    emp.Status = "Active";
                    try
                    {
                        _empBLL.Update(emp);
                        LoadEmployees();
                        // re-select the employee after reload
                        var reselected = _empBLL.GetAll().FirstOrDefault(x => x.EmployeeId == emp.EmployeeId);
                        if (reselected != null)
                        {
                            dgEmployees.SelectedItem = reselected;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
