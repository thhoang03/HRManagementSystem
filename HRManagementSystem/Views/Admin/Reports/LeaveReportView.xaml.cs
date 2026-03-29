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
    public partial class LeaveReportView : Page
    {
        private readonly LeaveRequestBLL _leaveBLL = new();
        private readonly DepartmentBLL _deptBLL = new();
        private List<LeaveReportRow> _reportData = new();

        public LeaveReportView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadYearCombo();
            LoadStatusCombo();
            LoadDepartmentCombo();
            LoadReport();
        }

        private void LoadYearCombo()
        {
            int currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 4, 6).Reverse().ToList();
            cbYear.ItemsSource = years;
            cbYear.SelectedValue = currentYear;
        }

        private void LoadStatusCombo()
        {
            cbStatus.ItemsSource = new[] { "All", "Pending", "Approved", "Rejected" };
            cbStatus.SelectedIndex = 0;
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

        private void LoadReport()
        {
            int year = cbYear.SelectedValue is int y ? y : DateTime.Now.Year;
            string status = cbStatus.SelectedItem as string ?? "All";
            int deptId = cbDepartment.SelectedValue is int d ? d : 0;

            var leaves = _leaveBLL.GetAll()
                .Where(l => l.StartDate.Year == year || l.EndDate.Year == year)
                .ToList();

            if (status != "All")
                leaves = leaves.Where(l => string.Equals(l.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();

            if (deptId > 0)
                leaves = leaves.Where(l => l.Employee?.DepartmentId == deptId).ToList();

            _reportData = leaves.Select(l => new LeaveReportRow
            {
                FullName = l.Employee?.FullName ?? "N/A",
                Department = l.Employee?.Department?.DepartmentName ?? "N/A",
                LeaveType = l.LeaveType ?? "N/A",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                DaysCount = (l.EndDate - l.StartDate).Days + 1,
                Status = l.Status ?? "N/A",
                ApprovedBy = l.ApprovedByNavigation?.FullName ?? "-"
            })
            .OrderByDescending(r => r.StartDate)
            .ToList();

            dgReport.ItemsSource = null;
            dgReport.ItemsSource = _reportData;

            txtTotal.Text = _reportData.Count.ToString();
            txtApproved.Text = _reportData.Count(r => r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)).ToString();
            txtRejected.Text = _reportData.Count(r => r.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase)).ToString();
            txtPending.Text = _reportData.Count(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToString();
            txtTotalDays.Text = _reportData
                .Where(r => r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.DaysCount).ToString();
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
                FileName = $"LeaveReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dlg.ShowDialog() != true) return;

            using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
            sw.WriteLine("FullName,Department,LeaveType,StartDate,EndDate,Days,Status,ApprovedBy");
            foreach (var r in _reportData)
                sw.WriteLine($"{r.FullName},{r.Department},{r.LeaveType},{r.StartDate:dd/MM/yyyy},{r.EndDate:dd/MM/yyyy},{r.DaysCount},{r.Status},{r.ApprovedBy}");

            MessageBox.Show($"Exported successfully to:\n{dlg.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class LeaveReportRow
        {
            public string FullName { get; set; } = "";
            public string Department { get; set; } = "";
            public string LeaveType { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int DaysCount { get; set; }
            public string Status { get; set; } = "";
            public string ApprovedBy { get; set; } = "";
        }
    }
}
