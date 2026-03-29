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
    public partial class AttendanceReportView : Page
    {
        private readonly AttendanceBLL _attBLL = new();
        private readonly DepartmentBLL _deptBLL = new();
        private List<AttendanceReportRow> _reportData = new();

        public AttendanceReportView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMonthCombo();
            LoadYearCombo();
            LoadDepartmentCombo();
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
            int currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 4, 6).Reverse().ToList();
            cbYear.ItemsSource = years;
            cbYear.SelectedValue = currentYear;
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
            int month = cbMonth.SelectedValue is int m ? m : 0;
            int year = cbYear.SelectedValue is int y ? y : DateTime.Now.Year;
            int deptId = cbDepartment.SelectedValue is int d ? d : 0;

            var attendances = _attBLL.GetAll();

            if (month > 0)
                attendances = attendances
                    .Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value.Month == month)
                    .ToList();

            attendances = attendances
                .Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value.Year == year)
                .ToList();

            if (deptId > 0)
                attendances = attendances
                    .Where(a => a.Employee?.DepartmentId == deptId)
                    .ToList();

            _reportData = attendances
                .GroupBy(a => a.EmployeeId)
                .Select(g =>
                {
                    var emp = g.First().Employee;
                    int present = g.Count(a => IsStatus(a.Status, "Present"));
                    int absent = g.Count(a => IsStatus(a.Status, "Absent"));
                    int late = g.Count(a => IsStatus(a.Status, "Late"));
                    int leave = g.Count(a => IsStatus(a.Status, "Leave"));
                    int total = g.Count();
                    double rate = total > 0 ? (double)present / total * 100 : 0;

                    return new AttendanceReportRow
                    {
                        EmployeeId = g.Key ?? 0,
                        FullName = emp?.FullName ?? "N/A",
                        Department = emp?.Department?.DepartmentName ?? "N/A",
                        PresentDays = present,
                        AbsentDays = absent,
                        LateDays = late,
                        LeaveDays = leave,
                        TotalRecords = total,
                        AttendanceRate = rate
                    };
                })
                .OrderBy(r => r.FullName)
                .ToList();

            dgReport.ItemsSource = null;
            dgReport.ItemsSource = _reportData;

            txtTotalPresent.Text = _reportData.Sum(r => r.PresentDays).ToString();
            txtTotalAbsent.Text = _reportData.Sum(r => r.AbsentDays).ToString();
            txtTotalLate.Text = _reportData.Sum(r => r.LateDays).ToString();
            txtTotalRecords.Text = _reportData.Sum(r => r.TotalRecords).ToString();
            txtRowCount.Text = $"{_reportData.Count} employee(s)";
        }

        private static bool IsStatus(string? status, string target)
            => string.Equals(status, target, StringComparison.OrdinalIgnoreCase);

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
                FileName = $"AttendanceReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (dlg.ShowDialog() != true) return;

            using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);
            sw.WriteLine("EmployeeId,FullName,Department,PresentDays,AbsentDays,LateDays,LeaveDays,TotalRecords,AttendanceRate(%)");
            foreach (var r in _reportData)
                sw.WriteLine($"{r.EmployeeId},{r.FullName},{r.Department},{r.PresentDays},{r.AbsentDays},{r.LateDays},{r.LeaveDays},{r.TotalRecords},{r.AttendanceRate:F1}");

            MessageBox.Show($"Exported successfully to:\n{dlg.FileName}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class AttendanceReportRow
        {
            public int EmployeeId { get; set; }
            public string FullName { get; set; } = "";
            public string Department { get; set; } = "";
            public int PresentDays { get; set; }
            public int AbsentDays { get; set; }
            public int LateDays { get; set; }
            public int LeaveDays { get; set; }
            public int TotalRecords { get; set; }
            public double AttendanceRate { get; set; }
        }
    }
}
