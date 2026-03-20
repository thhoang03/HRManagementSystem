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
    /// Interaction logic for ContractsView.xaml
    /// </summary>
    public partial class ContractsView : Page
    {
        private readonly ContractBLL _contBLL = new();
        private readonly EmployeeBLL _empBLL = new();

        public ContractsView()
        {
            InitializeComponent();
        }

        private void frmContracts_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            FillDgContracts();
        }

        private void FillDgContracts()
        {
            dgContracts.ItemsSource = null;
            dgContracts.ItemsSource = _contBLL.GetAll();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            txtContractType.Text = string.Empty;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtSalary.Text = string.Empty;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            cbEmployees.Focus();
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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (!TryGetContractInput(out ContractInput input))
                {
                    return;
                }

                contract.EmployeeId = input.EmployeeId;
                contract.ContractType = input.ContractType;
                contract.StartDate = input.StartDate;
                contract.EndDate = input.EndDate;
                contract.ContractSalary = input.ContractSalary;

                _contBLL.Update(contract);
                FillDgContracts();
                Clear();
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

            _contBLL.Add(contract);
            FillDgContracts();
            Clear();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to deactivate this contract?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    contract.Status = "Deactive";
                    _contBLL.Update(contract);
                    FillDgContracts();
                    Clear();
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to active this contract?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    contract.Status = "Active";
                    _contBLL.Update(contract);
                    FillDgContracts();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgContracts.ItemsSource = null;
            dgContracts.ItemsSource = _contBLL.Search(keyword);
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

            input = new ContractInput
            {
                EmployeeId = empId,
                ContractType = string.IsNullOrWhiteSpace(txtContractType.Text) ? null : txtContractType.Text.Trim(),
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
