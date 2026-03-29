using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.HR
{
    /// <summary>
    /// Interaction logic for ContractsView.xaml
    /// </summary>
    public partial class ContractsView : Page
    {
        private readonly ContractBLL _contBLL = new();
        private readonly EmployeeBLL _empBLL = new();
        private readonly int? _preselectEmployeeId;

        public ContractsView()
        {
            InitializeComponent();
        }

        public ContractsView(int preselectEmployeeId) : this()
        {
            _preselectEmployeeId = preselectEmployeeId;
        }

        private void ContractsView_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            if (_preselectEmployeeId.HasValue)
            {
                cbEmployees.SelectedValue = _preselectEmployeeId.Value;
            }
            FillDgContracts();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        public void SelectEmployeeById(int employeeId)
        {
            FillComboEmployees();
            cbEmployees.SelectedValue = employeeId;
        }

        private void FillDgContracts()
        {
            dgContracts.ItemsSource = null;
            // show only active contracts
            var all = _contBLL.GetAll();
            var visibles = all.Where(c => string.IsNullOrWhiteSpace(c.Status) || !c.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)).ToList();
            dgContracts.ItemsSource = visibles;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // clear search and reload employees/contracts
            try
            {
                var selected = cbEmployees.SelectedValue;
                if (txtSearch != null) txtSearch.Text = string.Empty;
                FillComboEmployees();
                if (selected != null) cbEmployees.SelectedValue = selected;
                FillDgContracts();
                if (dgContracts != null) dgContracts.SelectedItem = null;
                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                FillDgContracts();
                return;
            }

            // search contracts by employee name, type or status
            var results = _contBLL.Search(keyword);
            dgContracts.ItemsSource = results.Where(c => string.IsNullOrWhiteSpace(c.Status) || !c.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void dgContracts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                cbEmployees.SelectedValue = contract.EmployeeId;
                txtContractType.Text = contract.ContractType;
                dpStartDate.SelectedDate = contract.StartDate.ToDateTime(TimeOnly.MinValue);
                dpEndDate.SelectedDate = contract.EndDate?.ToDateTime(TimeOnly.MinValue);
                txtSalary.Text = contract.ContractSalary?.ToString();
            }
        }

        private void cbEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var emp = cbEmployees.SelectedItem as HRManagementSystem.Models.Employee;
            if (emp == null) return;

            if (!string.IsNullOrWhiteSpace(emp.Status) && emp.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                if (MessageBox.Show($"Nhân viên '{emp.FullName}' hiện Inactive. Bạn có muốn active nhân viên này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    emp.Status = "Active";
                    try
                    {
                        _empBLL.Update(emp);
                        FillComboEmployees();
                        cbEmployees.SelectedValue = emp.EmployeeId;
                        MessageBox.Show("Employee activated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetContractInput(out ContractInput input))
            {
                return;
            }

            Contract contract = new();
            contract.EmployeeId = input.EmployeeId;
            contract.ContractType = input.ContractType;
            contract.StartDate = input.StartDate;
            contract.EndDate = input.EndDate;
            contract.ContractSalary = input.ContractSalary;
            contract.Status = "Active";

            _contBLL.Add(contract);
            FillDgContracts();
            Clear();
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            txtContractType.Text = string.Empty;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtSalary.Text = string.Empty;
        }

        private void btnDeleteContract_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to deactivate this contract?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    contract.Status = "Deactive";
                    _contBLL.Update(contract);
                    FillDgContracts();
                    Clear();
                }
            }
        }

        private bool TryGetContractInput(out ContractInput input)
        {
            input = new ContractInput();

            if (cbEmployees.SelectedValue == null || !int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId))
            {
                MessageBox.Show("Please select a valid employee.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbEmployees.Focus();
                return false;
            }

            if (!dpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Start date is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return false;
            }

            DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
            DateOnly? endDate = null;
            if (dpEndDate.SelectedDate.HasValue)
            {
                endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                if (endDate.Value < startDate)
                {
                    MessageBox.Show("End date cannot be earlier than start date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpEndDate.Focus();
                    return false;
                }
            }

            decimal? salary = null;
            string salaryText = txtSalary.Text.Trim();
            if (!string.IsNullOrWhiteSpace(salaryText))
            {
                if (!decimal.TryParse(salaryText, out decimal salaryValue))
                {
                    MessageBox.Show("Salary must be a valid number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtSalary.Focus();
                    return false;
                }

                if (salaryValue <= 0)
                {
                    MessageBox.Show("Salary must be greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtSalary.Focus();
                    return false;
                }

                salary = salaryValue;
            }

            string contractType = string.IsNullOrWhiteSpace(txtContractType.Text) ? string.Empty : txtContractType.Text.Trim();

            // Contract type validation: allow letters and common punctuation (letters, digits, spaces, '-', '_')
            if (string.IsNullOrWhiteSpace(contractType) || !contractType.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_'))
            {
                MessageBox.Show("Contract Type is required and must contain letters, digits, spaces, '-' or '_'.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtContractType.Focus();
                return false;
            }

            input = new ContractInput
            {
                EmployeeId = empId,
                ContractType = contractType,
                StartDate = startDate,
                EndDate = endDate,
                ContractSalary = salary
            };

            return true;
        }

        private sealed class ContractInput
        {
            public int EmployeeId { get; set; }
            public string? ContractType { get; set; }
            public DateOnly StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public decimal? ContractSalary { get; set; }
        }
    }
}
