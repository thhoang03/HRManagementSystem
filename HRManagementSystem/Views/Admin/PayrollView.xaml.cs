using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for PayrollView.xaml
    /// </summary>
    public partial class PayrollView : Page
    {
        private readonly PayrollBLL _payBLL = new();
        private readonly EmployeeBLL _empBLL = new();
        private readonly ContractBLL _contBLL = new();

        public PayrollView()
        {
            InitializeComponent();
        }

        private void frmPayroll_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            FillComboContracts();
            FillFilterEmployees();
            FillMonthCombos();
            FillYearCombos();
            FillDataGridPayroll();
            dpCreatedAt.SelectedDate = DateTime.Now;
        }

        private void FillDataGridPayroll()
        {
            dgPayroll.ItemsSource = null;
            dgPayroll.ItemsSource = _payBLL.GetAll();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        private void FillFilterEmployees()
        {
            cbFilterEmployees.ItemsSource = null;
            cbFilterEmployees.ItemsSource = _empBLL.GetAll();
            cbFilterEmployees.SelectedValuePath = "EmployeeId";
            cbFilterEmployees.DisplayMemberPath = "FullName";
        }

        private void FillComboContracts(int? employeeId = null)
        {
            var contracts = _contBLL.GetAll();

            if (employeeId.HasValue)
            {
                contracts = contracts.Where(c => c.EmployeeId == employeeId.Value).ToList();
            }

            cbContracts.ItemsSource = null;
            cbContracts.ItemsSource = contracts;
            cbContracts.SelectedValuePath = "ContractId";
            cbContracts.DisplayMemberPath = "ContractType";
        }

        private void FillMonthCombos()
        {
            var months = Enumerable.Range(1, 12)
                .Select(m => new MonthItem(m, new DateTime(2000, m, 1).ToString("MMMM")))
                .ToList();

            cbMonth.ItemsSource = null;
            cbMonth.ItemsSource = months;
            cbMonth.DisplayMemberPath = "Name";
            cbMonth.SelectedValuePath = "Value";

            cbFilterMonth.ItemsSource = null;
            cbFilterMonth.ItemsSource = months;
            cbFilterMonth.DisplayMemberPath = "Name";
            cbFilterMonth.SelectedValuePath = "Value";
        }

        private void FillYearCombos()
        {
            var years = _payBLL.GetAll()
                .Where(p => p.Year.HasValue)
                .Select(p => p.Year!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (years.Count == 0)
            {
                years.Add(DateTime.Now.Year);
            }
            else if (!years.Contains(DateTime.Now.Year))
            {
                years.Insert(0, DateTime.Now.Year);
            }

            cbYear.ItemsSource = null;
            cbYear.ItemsSource = years;

            cbFilterYear.ItemsSource = null;
            cbFilterYear.ItemsSource = years;
        }

        private void ApplyFilters()
        {
            IEnumerable<Payroll> data = _payBLL.GetAll();

            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(p =>
                    (p.Employee != null && p.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(p.Status) && p.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (p.Contract != null && !string.IsNullOrEmpty(p.Contract.ContractType)
                        && p.Contract.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (cbFilterEmployees.SelectedValue != null
                && int.TryParse(cbFilterEmployees.SelectedValue.ToString(), out int empId))
            {
                data = data.Where(p => p.EmployeeId == empId);
            }

            if (cbFilterMonth.SelectedValue != null
                && int.TryParse(cbFilterMonth.SelectedValue.ToString(), out int month))
            {
                data = data.Where(p => p.Month == month);
            }

            if (cbFilterYear.SelectedValue != null
                && int.TryParse(cbFilterYear.SelectedValue.ToString(), out int year))
            {
                data = data.Where(p => p.Year == year);
            }

            string status = cbFilterStatus.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(status))
            {
                data = data.Where(p => !string.IsNullOrEmpty(p.Status)
                                    && p.Status!.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            dgPayroll.ItemsSource = null;
            dgPayroll.ItemsSource = data.ToList();
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            cbContracts.SelectedValue = null;
            cbMonth.SelectedValue = null;
            cbYear.SelectedValue = null;
            txtBaseSalary.Text = string.Empty;
            txtOvertimePay.Text = string.Empty;
            txtDeduction.Text = string.Empty;
            txtBonus.Text = string.Empty;
            txtNetSalary.Text = string.Empty;
            cbStatus.SelectedIndex = -1;
            dpCreatedAt.SelectedDate = DateTime.Now;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            cbEmployees.Focus();
        }

        private void cbEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbEmployees.SelectedValue != null
                && int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId))
            {
                FillComboContracts(empId);
            }
            else
            {
                FillComboContracts();
            }
        }

        private void dgPayroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var payroll = dgPayroll.SelectedItem as Payroll;
            if (payroll != null)
            {
                cbEmployees.SelectedValue = payroll.EmployeeId;
                FillComboContracts(payroll.EmployeeId);
                cbContracts.SelectedValue = payroll.ContractId;
                cbMonth.SelectedValue = payroll.Month;
                cbYear.SelectedValue = payroll.Year;
                txtBaseSalary.Text = payroll.BaseSalary?.ToString() ?? string.Empty;
                txtOvertimePay.Text = payroll.OvertimePay?.ToString() ?? string.Empty;
                txtDeduction.Text = payroll.Deduction?.ToString() ?? string.Empty;
                txtBonus.Text = payroll.Bonus?.ToString() ?? string.Empty;
                txtNetSalary.Text = payroll.NetSalary?.ToString() ?? string.Empty;
                cbStatus.Text = payroll.Status ?? string.Empty;
                dpCreatedAt.SelectedDate = payroll.CreatedAt;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cbEmployees.SelectedValue == null || cbMonth.SelectedValue == null || cbYear.SelectedValue == null)
            {
                return;
            }

            int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId);
            int? contractId = null;
            if (cbContracts.SelectedValue != null && int.TryParse(cbContracts.SelectedValue.ToString(), out int contId))
            {
                contractId = contId;
            }

            int.TryParse(cbMonth.SelectedValue.ToString(), out int month);
            int.TryParse(cbYear.SelectedValue.ToString(), out int year);

            decimal? baseSalary = ParseDecimal(txtBaseSalary.Text);
            decimal? overtime = ParseDecimal(txtOvertimePay.Text);
            decimal? deduction = ParseDecimal(txtDeduction.Text);
            decimal? bonus = ParseDecimal(txtBonus.Text);

            decimal netSalary = CalculateNetSalary(baseSalary, overtime, bonus, deduction);

            Payroll payroll = new();
            payroll.EmployeeId = empId;
            payroll.ContractId = contractId;
            payroll.Month = month;
            payroll.Year = year;
            payroll.BaseSalary = baseSalary;
            payroll.OvertimePay = overtime;
            payroll.Deduction = deduction;
            payroll.Bonus = bonus;
            payroll.NetSalary = netSalary;
            payroll.Status = cbStatus.Text?.Trim();
            payroll.CreatedAt = dpCreatedAt.SelectedDate ?? DateTime.Now;

            _payBLL.Add(payroll);
            FillDataGridPayroll();
            Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var payroll = dgPayroll.SelectedItem as Payroll;
            if (payroll != null)
            {
                if (cbEmployees.SelectedValue == null || cbMonth.SelectedValue == null || cbYear.SelectedValue == null)
                {
                    return;
                }

                int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId);
                int? contractId = null;
                if (cbContracts.SelectedValue != null && int.TryParse(cbContracts.SelectedValue.ToString(), out int contId))
                {
                    contractId = contId;
                }

                int.TryParse(cbMonth.SelectedValue.ToString(), out int month);
                int.TryParse(cbYear.SelectedValue.ToString(), out int year);

                decimal? baseSalary = ParseDecimal(txtBaseSalary.Text);
                decimal? overtime = ParseDecimal(txtOvertimePay.Text);
                decimal? deduction = ParseDecimal(txtDeduction.Text);
                decimal? bonus = ParseDecimal(txtBonus.Text);

                decimal netSalary = CalculateNetSalary(baseSalary, overtime, bonus, deduction);

                payroll.EmployeeId = empId;
                payroll.ContractId = contractId;
                payroll.Month = month;
                payroll.Year = year;
                payroll.BaseSalary = baseSalary;
                payroll.OvertimePay = overtime;
                payroll.Deduction = deduction;
                payroll.Bonus = bonus;
                payroll.NetSalary = netSalary;
                payroll.Status = cbStatus.Text?.Trim();
                payroll.CreatedAt = dpCreatedAt.SelectedDate ?? DateTime.Now;

                _payBLL.Update(payroll);
                FillDataGridPayroll();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var payroll = dgPayroll.SelectedItem as Payroll;
            if (payroll != null)
            {
                if (MessageBox.Show("Do you really want to delete this payroll?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _payBLL.Delete(payroll);
                    FillDataGridPayroll();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cbFilterEmployees.SelectedValue = null;
            cbFilterMonth.SelectedValue = null;
            cbFilterYear.SelectedValue = null;
            cbFilterStatus.SelectedIndex = -1;
            txtSearch.Text = string.Empty;
            FillDataGridPayroll();
        }

        private void SalaryFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateNetSalary();
        }

        private void RecalculateNetSalary()
        {
            decimal baseSalary = GetDecimalOrZero(txtBaseSalary.Text);
            decimal overtime = GetDecimalOrZero(txtOvertimePay.Text);
            decimal deduction = GetDecimalOrZero(txtDeduction.Text);
            decimal bonus = GetDecimalOrZero(txtBonus.Text);

            decimal net = baseSalary + overtime + bonus - deduction;
            txtNetSalary.Text = net.ToString("N2");
        }

        private static decimal CalculateNetSalary(decimal? baseSalary, decimal? overtime, decimal? bonus, decimal? deduction)
        {
            return (baseSalary ?? 0m) + (overtime ?? 0m) + (bonus ?? 0m) - (deduction ?? 0m);
        }

        private static decimal? ParseDecimal(string text)
        {
            if (decimal.TryParse(text.Trim(), out decimal value))
            {
                return value;
            }

            return null;
        }

        private static decimal GetDecimalOrZero(string text)
        {
            if (decimal.TryParse(text.Trim(), out decimal value))
            {
                return value;
            }

            return 0m;
        }

        private sealed class MonthItem
        {
            public int Value { get; }
            public string Name { get; }

            public MonthItem(int value, string name)
            {
                Value = value;
                Name = name;
            }
        }
    }
}
