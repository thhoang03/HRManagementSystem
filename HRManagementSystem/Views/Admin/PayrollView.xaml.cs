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

            var visibleRows = data.ToList();
            dgPayroll.ItemsSource = null;
            dgPayroll.ItemsSource = visibleRows;
            UpdateHeaderActionVisibility(visibleRows);
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
                return;
            }

            var previews = _payBLL.GeneratePreview(month, year);
            ShowGenerated(previews);
        }

        private void btnSavePayroll_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                MessageBox.Show("Please select month and year.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_showingGenerated || _generatedRows.Count == 0)
            {
                MessageBox.Show("No generated preview to save. Please click Generate first.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                $"Do you want to save all payrolls for {month}/{year}?",
                "Confirm Save All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var rowsToSave = _generatedRows
                .Where(p => !_payBLL.HasPayroll(month, year, p.EmployeeId))
                .ToList();

            if (rowsToSave.Count == 0)
            {
                MessageBox.Show("All payrolls are already saved.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPayrollsFromDb(month, year);
                return;
            }

            _payBLL.SavePayrolls(rowsToSave);
            MessageBox.Show($"Save All completed. Saved {rowsToSave.Count} payroll(s).", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPayrollsFromDb(month, year);
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            UpdateAllStatus("Approved");
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            UpdateAllStatus("Paid");
        }

        private void UpdateAllStatus(string status)
        {
            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                MessageBox.Show("Please select month and year.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                $"Do you want to {status.ToLowerInvariant()} all payrolls for {month}/{year}?",
                $"Confirm {status} All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_showingGenerated && _generatedRows.Count > 0)
            {
                var rowsToSave = _generatedRows
                    .Where(p => !_payBLL.HasPayroll(month, year, p.EmployeeId))
                    .ToList();

                if (rowsToSave.Count > 0)
                {
                    _payBLL.SavePayrolls(rowsToSave);
                }
            }

            if (!_payBLL.HasPayrolls(month, year))
            {
                MessageBox.Show("No payrolls found for this month.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                && !_currentRows.Any(p => IsStatus(p.Status, "Draft")))
            {
                MessageBox.Show("No Draft payroll available to approve.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                && !_currentRows.Any(p => IsStatus(p.Status, "Approved")))
            {
                MessageBox.Show("No Approved payroll available to pay.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int affectedAll = status.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                ? _payBLL.UpdateStatus(month, year, "Draft", "Approved")
                : _payBLL.UpdateStatus(month, year, "Approved", "Paid");
            MessageBox.Show($"{status} All completed. Updated {affectedAll} payroll(s).", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPayrollsFromDb(month, year);
        }

        private void btnRowSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetRowPayroll(sender, out PayrollPreview payroll))
            {
                MessageBox.Show("Cannot identify selected payroll row.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show(
                $"Do you want to save payroll for {payroll.EmployeeName} ({payroll.Month}/{payroll.Year})?",
                "Confirm Save",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_payBLL.HasPayroll(payroll.Month, payroll.Year, payroll.EmployeeId))
            {
                MessageBox.Show("This payroll is already saved.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPayrollsFromDb(payroll.Month, payroll.Year);
                return;
            }

            _payBLL.SavePayrolls(new[] { payroll });
            MessageBox.Show($"Saved payroll for {payroll.EmployeeName}.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPayrollsFromDb(payroll.Month, payroll.Year);
        }

        private void btnRowDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetRowPayroll(sender, out PayrollPreview payroll))
            {
                MessageBox.Show("Cannot identify selected payroll row.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService?.Navigate(new PayrollDetailView(payroll));
        }

        private void btnRowApprove_Click(object sender, RoutedEventArgs e)
        {
            UpdateSingleStatus(sender, "Approved");
        }

        private void btnRowPay_Click(object sender, RoutedEventArgs e)
        {
            UpdateSingleStatus(sender, "Paid");
        }

        private void UpdateSingleStatus(object sender, string status)
        {
            if (!TryGetRowPayroll(sender, out PayrollPreview payroll))
            {
                MessageBox.Show("Cannot identify selected payroll row.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show(
                $"Do you want to {status.ToLowerInvariant()} payroll for {payroll.EmployeeName} ({payroll.Month}/{payroll.Year})?",
                $"Confirm {status}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase) && !IsStatus(payroll.Status, "Draft"))
            {
                MessageBox.Show("Only payroll with Draft status can be approved.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase) && !IsStatus(payroll.Status, "Approved"))
            {
                MessageBox.Show("Only payroll with Approved status can be paid.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_payBLL.HasPayroll(payroll.Month, payroll.Year, payroll.EmployeeId))
            {
                _payBLL.SavePayrolls(new[] { payroll });
            }

            int affected = _payBLL.UpdateStatus(payroll.Month, payroll.Year, payroll.EmployeeId, status);
            if (affected == 0)
            {
                MessageBox.Show("No saved payroll found for selected row.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show($"{status} successful for {payroll.EmployeeName}.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadPayrollsFromDb(payroll.Month, payroll.Year);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void cbGenMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadBySelectedPeriod();
        }

        private void cbGenYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadBySelectedPeriod();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            ApplySearch();
        }

        private void ReloadBySelectedPeriod()
        {
            if (!IsLoaded)
            {
                return;
            }

            if (!TryGetSelectedMonthYear(out int month, out int year))
            {
                return;
            }

            LoadPayrollsFromDb(month, year);
        }

        private bool TryGetRowPayroll(object sender, out PayrollPreview payroll)
        {
            payroll = null!;
            if (sender is Button button && button.DataContext is PayrollPreview row)
            {
                payroll = row;
                return true;
            }

            return false;
        }

        private static bool IsStatus(string? current, string expected)
        {
            return string.Equals(current?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateHeaderActionVisibility(List<PayrollPreview> visibleRows)
        {
            btnSavePayroll.Visibility = _showingGenerated && _generatedRows.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            btnApprove.Visibility = visibleRows.Any(p => IsStatus(p.Status, "Draft"))
                ? Visibility.Visible
                : Visibility.Collapsed;

            btnPay.Visibility = visibleRows.Any(p => IsStatus(p.Status, "Approved"))
                ? Visibility.Visible
                : Visibility.Collapsed;
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
