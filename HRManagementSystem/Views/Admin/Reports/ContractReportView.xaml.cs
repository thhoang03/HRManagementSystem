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
    public partial class ContractReportView : Page
    {
        private readonly ContractBLL _contBLL = new();
        private readonly DepartmentBLL _deptBLL = new();
        private List<ContractReportRow> _reportData = new();

        public ContractReportView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatusCombo();
            LoadContractTypeCombo();
            LoadDepartmentCombo();
            LoadExpiryCombo();
            LoadReport();
        }

        private void LoadStatusCombo()
        {
            cbStatus.ItemsSource = new[] { "All", "Active", "Expired", "Terminated" };
            cbStatus.SelectedIndex = 0;
        }

        private void LoadContractTypeCombo()
        {
            var types = _contBLL.GetAll()
                .Where(c => !string.IsNullOrWhiteSpace(c.ContractType))
                .Select(c => c.ContractType!)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var all = new List<string> { "All" };
            all.AddRange(types);
            cbContractType.ItemsSource = all;
            cbContractType.SelectedIndex = 0;
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

        private void LoadExpiryCombo()
        {
            var options = new[]
            {
                new { Value = 0,   Display = "All" },
                new { Value = 30,  Display = "30 days" },
                new { Value = 60,  Display = "60 days" },
                new { Value = 90,  Display = "90 days" },
                new { Value = 180, Display = "180 days" }
            };
            cbExpiry.ItemsSource = options;
            cbExpiry.DisplayMemberPath = "Display";
            cbExpiry.SelectedValuePath = "Value";
            cbExpiry.SelectedValue = 0;
        }

        private void LoadReport()
        {
            string status = cbStatus.SelectedItem as string ?? "All";
            string contractType = cbContractType.SelectedItem as string ?? "All";
            int deptId = cbDepartment.SelectedValue is int d ? d : 0;
            int expiryDays = cbExpiry.SelectedValue is int ex ? ex : 0;

            var contracts = _contBLL.GetAll();

            if (status != "All")
                contracts = contracts
                    .Where(c => string.Equals(c.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (contractType != "All")
                contracts = contracts
                    .Where(c => string.Equals(c.ContractType, contractType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (deptId > 0)
                contracts = contracts.Where(c => c.Employee?.DepartmentId == deptId).ToList();

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (expiryDays > 0)
            {
                var deadline = today.AddDays(expiryDays);
                contracts = contracts
                    .Where(c => c.EndDate.HasValue && c.EndDate.Value >= today && c.EndDate.Value <= deadline)
                    .ToList();
            }

            _reportData = contracts.Select(c =>
            {
                int? daysRemaining = c.EndDate.HasValue
                    ? c.EndDate.Value.DayNumber - today.DayNumber
                    : (int?)null;

                return new ContractReportRow
                {
                    FullName = c.Employee?.FullName ?? "N/A",
                    Department = c.Employee?.Department?.DepartmentName ?? "N/A",
                    ContractType = c.ContractType ?? "N/A",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value.ToString("dd/MM/yyyy") : "No end date",
                    Salary = c.ContractSalary ?? 0,
                    Status = c.Status ?? "N/A",
                    DaysRemaining = daysRemaining.HasValue
                        ? (daysRemaining.Value < 0 ? "Expired" : daysRemaining.Value.ToString())
                        : "∞"
                };
            })
            .OrderBy(r => r.FullName)
            .ToList();

            dgReport.ItemsSource = null;
            dgReport.ItemsSource = _reportData;

            // Summary cards — base on ALL contracts (no expiry filter for global stats)
            var allContracts = _contBLL.GetAll();
            txtTotal.Text = _reportData.Count.ToString();
            txtActive.Text = _reportData.Count(r => r.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).ToString();
            txtExpired.Text = _reportData.Count(r => r.Status.Equals("Expired", StringComparison.OrdinalIgnoreCase)).ToString();

            var deadline30 = today.AddDays(30);
            txtExpiringSoon.Text = allContracts
                .Count(c => c.EndDate.HasValue && c.EndDate.Value >= today && c.EndDate.Value <= deadline30
                         && string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .ToString();

            txtTotalSalary.Text = $"{_reportData.Where(r => r.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)).Sum(r => r.Salary):N0}";
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
                FileName = $"ContractReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dlg.ShowDialog() != true) return;

            using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
            sw.WriteLine("FullName,Department,ContractType,StartDate,EndDate,Salary,Status,DaysRemaining");
            foreach (var r in _reportData)
                sw.WriteLine($"{r.FullName},{r.Department},{r.ContractType},{r.StartDate:dd/MM/yyyy},{r.EndDate},{r.Salary:N0},{r.Status},{r.DaysRemaining}");

            MessageBox.Show($"Exported successfully to:\n{dlg.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class ContractReportRow
        {
            public string FullName { get; set; } = "";
            public string Department { get; set; } = "";
            public string ContractType { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public string EndDate { get; set; } = "";
            public decimal Salary { get; set; }
            public string Status { get; set; } = "";
            public string DaysRemaining { get; set; } = "";
        }
    }
}
