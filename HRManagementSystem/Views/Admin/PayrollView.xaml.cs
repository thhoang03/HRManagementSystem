using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly List<PayrollPreview> _generatedRows = new();
        private List<PayrollPreview> _currentRows = new();
        private bool _showingGenerated;

        public PayrollView()
        {
            InitializeComponent();
        }

        private void frmPayroll_Loaded(object sender, RoutedEventArgs e)
        {
            FillMonthCombo();
            FillYearCombo();

            cbGenMonth.SelectedValue = DateTime.Now.Month;
            cbGenYear.SelectedValue = DateTime.Now.Year;

            LoadPayrollsFromDb(DateTime.Now.Month, DateTime.Now.Year);
        }

        private void FillMonthCombo()
        {
            var months = Enumerable.Range(1, 12)
                .Select(m => new MonthItem(m, new DateTime(2000, m, 1).ToString("MMMM", CultureInfo.InvariantCulture)))
                .ToList();

            cbGenMonth.ItemsSource = null;
            cbGenMonth.ItemsSource = months;
            cbGenMonth.DisplayMemberPath = "Name";
            cbGenMonth.SelectedValuePath = "Value";
        }

        private void FillYearCombo()
        {
            int currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 2, 5).ToList();

            cbGenYear.ItemsSource = null;
            cbGenYear.ItemsSource = years;
        }

        private void LoadPayrollsFromDb(int month, int year)
        {
            _currentRows = _payBLL.GetPayrollPreviews(month, year);
            _showingGenerated = false;
            txtMode.Text = _currentRows.Count == 0 ? "No payrolls found" : "Saved payrolls";
            ApplySearch();
        }

        private void ShowGenerated(List<PayrollPreview> rows)
        {
            _generatedRows.Clear();
            _generatedRows.AddRange(rows);
            _currentRows = _generatedRows.ToList();
            _showingGenerated = true;
            txtMode.Text = "Preview (not saved)";
            ApplySearch();
        }

        private void ApplySearch()
        {
            string keyword = txtSearch.Text.Trim();
            IEnumerable<PayrollPreview> data = _currentRows;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(p => p.EmployeeName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            dgPayroll.ItemsSource = null;
            dgPayroll.ItemsSource = data.ToList();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                MessageBox.Show("Please select month and year.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_payBLL.HasPayrolls(month, year))
            {
                MessageBox.Show("Payroll for this month already exists. Loading saved payrolls.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPayrollsFromDb(month, year);
                ClearDetail();
                return;
            }

            var previews = _payBLL.GeneratePreview(month, year);
            ShowGenerated(previews);
            ClearDetail();
        }

        private void btnSavePayroll_Click(object sender, RoutedEventArgs e)
        {
            if (!_showingGenerated || _generatedRows.Count == 0)
            {
                MessageBox.Show("Please generate payroll first.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                MessageBox.Show("Please select month and year.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_payBLL.HasPayrolls(month, year))
            {
                MessageBox.Show("Payroll for this month already exists.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadPayrollsFromDb(month, year);
                ClearDetail();
                return;
            }

            _payBLL.SavePayrolls(_generatedRows);
            LoadPayrollsFromDb(month, year);
            ClearDetail();
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Approved");
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Paid");
        }

        private void UpdateStatus(string status)
        {
            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                MessageBox.Show("Please select month and year.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_payBLL.HasPayrolls(month, year))
            {
                MessageBox.Show("No payrolls found for this month.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _payBLL.UpdateStatus(month, year, status);
            LoadPayrollsFromDb(month, year);
            ClearDetail();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            ApplySearch();
        }

        private void dgPayroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPayroll.SelectedItem is PayrollPreview payroll)
            {
                FillDetail(payroll);
            }
        }

        private void FillDetail(PayrollPreview payroll)
        {
            txtEmployee.Text = payroll.EmployeeName;
            txtContract.Text = payroll.ContractType ?? string.Empty;
            txtMonth.Text = payroll.Month.ToString();
            txtYear.Text = payroll.Year.ToString();
            txtBaseSalary.Text = payroll.BaseSalary.ToString("N2");
            txtWorkDays.Text = payroll.WorkDays.ToString();
            txtLeaveDays.Text = payroll.LeaveDays.ToString();
            txtBonus.Text = payroll.Bonus.ToString("N2");
            txtOvertime.Text = payroll.OvertimePay.ToString("N2");
            txtDeduction.Text = payroll.Deduction.ToString("N2");
            txtNetSalary.Text = payroll.NetSalary.ToString("N2");
        }

        private void ClearDetail()
        {
            txtEmployee.Text = string.Empty;
            txtContract.Text = string.Empty;
            txtMonth.Text = string.Empty;
            txtYear.Text = string.Empty;
            txtBaseSalary.Text = string.Empty;
            txtWorkDays.Text = string.Empty;
            txtLeaveDays.Text = string.Empty;
            txtBonus.Text = string.Empty;
            txtOvertime.Text = string.Empty;
            txtDeduction.Text = string.Empty;
            txtNetSalary.Text = string.Empty;
        }

        private void DetailSalary_TextChanged(object sender, TextChangedEventArgs e)
        {
            decimal baseSalary = GetDecimalOrZero(txtBaseSalary.Text);
            decimal bonus = GetDecimalOrZero(txtBonus.Text);
            decimal overtime = GetDecimalOrZero(txtOvertime.Text);
            decimal deduction = GetDecimalOrZero(txtDeduction.Text);

            decimal net = baseSalary + bonus + overtime - deduction;
            txtNetSalary.Text = net.ToString("N2");
        }

        private void btnUpdateRow_Click(object sender, RoutedEventArgs e)
        {
            if (!_showingGenerated)
            {
                MessageBox.Show("You can only edit bonus/overtime in preview mode.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (dgPayroll.SelectedItem is not PayrollPreview payroll)
            {
                return;
            }

            payroll.Bonus = GetDecimalOrZero(txtBonus.Text);
            payroll.OvertimePay = GetDecimalOrZero(txtOvertime.Text);
            payroll.Deduction = GetDecimalOrZero(txtDeduction.Text);
            payroll.NetSalary = GetDecimalOrZero(txtNetSalary.Text);

            dgPayroll.Items.Refresh();
        }

        private bool TryGetSelectedMonthYear(out int month, out int year)
        {
            month = 0;
            year = 0;

            if (cbGenMonth.SelectedValue == null || cbGenYear.SelectedValue == null)
            {
                return false;
            }

            if (!int.TryParse(cbGenMonth.SelectedValue.ToString(), out month))
            {
                return false;
            }

            if (!int.TryParse(cbGenYear.SelectedValue.ToString(), out year))
            {
                return false;
            }

            return true;
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
