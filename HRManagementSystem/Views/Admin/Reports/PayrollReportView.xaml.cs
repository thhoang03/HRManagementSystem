using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace HRManagementSystem.Views.Admin.Reports
{
    public partial class PayrollReportView : Page
    {
        private readonly PayrollBLL _payBLL = new();
        private readonly DepartmentBLL _deptBLL = new();
        private List<PayrollReportRow> _reportData = new();

        public PayrollReportView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMonthCombo();
            LoadYearCombo();
            LoadDepartmentCombo();
            LoadStatusCombo();
            LoadReport();
        }

        private void LoadMonthCombo()
        {
            var months = new[]
            {
                new { Value = 0,  Display = "All Months" },
                new { Value = 1,  Display = "January" },
                new { Value = 2,  Display = "February" },
                new { Value = 3,  Display = "March" },
                new { Value = 4,  Display = "April" },
                new { Value = 5,  Display = "May" },
                new { Value = 6,  Display = "June" },
                new { Value = 7,  Display = "July" },
                new { Value = 8,  Display = "August" },
                new { Value = 9,  Display = "September" },
                new { Value = 10, Display = "October" },
                new { Value = 11, Display = "November" },
                new { Value = 12, Display = "December" }
            };
            cbMonth.ItemsSource = months;
            cbMonth.DisplayMemberPath = "Display";
            cbMonth.SelectedValuePath = "Value";
            cbMonth.SelectedValue = DateTime.Now.Month;
        }

        private void LoadYearCombo()
        {
            int cur = DateTime.Now.Year;
            cbYear.ItemsSource = Enumerable.Range(cur - 4, 6).Reverse().ToList();
            cbYear.SelectedValue = cur;
        }

        private void LoadDepartmentCombo()
        {
            var depts = _deptBLL.GetAll();
            var all = new List<object> { new { DepartmentId = 0, DepartmentName = "All Departments" } };
            all.AddRange(depts.Cast<object>());
            cbDepartment.ItemsSource = all;
            cbDepartment.DisplayMemberPath = "DepartmentName";
            cbDepartment.SelectedValuePath = "DepartmentId";
            cbDepartment.SelectedValue = 0;
        }

        private void LoadStatusCombo()
        {
            cbStatus.ItemsSource = new[] { "All", "Draft", "Confirmed" };
            cbStatus.SelectedIndex = 0;
        }

        private void LoadReport()
        {
            int month = cbMonth.SelectedValue is int m ? m : 0;
            int year = cbYear.SelectedValue is int y ? y : DateTime.Now.Year;
            int deptId = cbDepartment.SelectedValue is int d ? d : 0;
            string status = cbStatus.SelectedItem as string ?? "All";

            var payrolls = _payBLL.GetAll();

            if (month > 0)
                payrolls = payrolls.Where(p => p.Month == month).ToList();

            payrolls = payrolls.Where(p => p.Year == year).ToList();

            if (deptId > 0)
                payrolls = payrolls.Where(p => p.Employee?.DepartmentId == deptId).ToList();

            if (status != "All")
                payrolls = payrolls
                    .Where(p => string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            _reportData = payrolls.Select(p => new PayrollReportRow
            {
                FullName = p.Employee?.FullName ?? "N/A",
                Department = p.Employee?.Department?.DepartmentName ?? "N/A",
                MonthYear = $"{p.Month:D2}/{p.Year}",
                BaseSalary = p.BaseSalary ?? 0,
                OvertimePay = p.OvertimePay ?? 0,
                Bonus = p.Bonus ?? 0,
                Deduction = p.Deduction ?? 0,
                NetSalary = p.NetSalary ?? 0,
                Status = p.Status ?? "N/A"
            })
            .OrderBy(r => r.FullName)
            .ToList();

            dgReport.ItemsSource = null;
            dgReport.ItemsSource = _reportData;

            txtTotalBase.Text = $"{_reportData.Sum(r => r.BaseSalary):N0}";
            txtTotalNet.Text = $"{_reportData.Sum(r => r.NetSalary):N0}";
            txtTotalBonus.Text = $"{_reportData.Sum(r => r.Bonus):N0}";
            txtTotalDeduction.Text = $"{_reportData.Sum(r => r.Deduction):N0}";
            txtRowCount.Text = $"{_reportData.Count} record(s)";
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e) => LoadReport();

        private void btnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_reportData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"PayrollReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dlg.ShowDialog() != true) return;

            using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
            sw.WriteLine("FullName,Department,MonthYear,BaseSalary,OvertimePay,Bonus,Deduction,NetSalary,Status");
            foreach (var r in _reportData)
                sw.WriteLine($"{r.FullName},{r.Department},{r.MonthYear},{r.BaseSalary:N0},{r.OvertimePay:N0},{r.Bonus:N0},{r.Deduction:N0},{r.NetSalary:N0},{r.Status}");

            MessageBox.Show($"Exported successfully to:\n{dlg.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class PayrollReportRow
        {
            public string FullName { get; set; } = "";
            public string Department { get; set; } = "";
            public string MonthYear { get; set; } = "";
            public decimal BaseSalary { get; set; }
            public decimal OvertimePay { get; set; }
            public decimal Bonus { get; set; }
            public decimal Deduction { get; set; }
            public decimal NetSalary { get; set; }
            public string Status { get; set; } = "";
        }
    }
}
