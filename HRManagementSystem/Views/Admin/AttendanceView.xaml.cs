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
    /// Interaction logic for AttendanceView.xaml
    /// </summary>
    public partial class AttendanceView : Page
    {
        private readonly AttendanceBLL _attBLL = new();
        private readonly EmployeeBLL _empBLL = new();

        public AttendanceView()
        {
            InitializeComponent();
        }

        private void frmAttendance_Loaded(object sender, RoutedEventArgs e)
        {
            FillFilterEmployees();
            FillFilterMonths();
            FillFilterYears();
            FillDataGridAttendance();
        }

        private void FillFilterEmployees()
        {
            cbFilterEmployees.ItemsSource = null;
            cbFilterEmployees.ItemsSource = _empBLL.GetAll();
            cbFilterEmployees.SelectedValuePath = "EmployeeId";
            cbFilterEmployees.DisplayMemberPath = "FullName";
        }

        private void FillFilterMonths()
        {
            var months = Enumerable.Range(1, 12)
                .Select(m => new MonthItem(m, new DateTime(2000, m, 1).ToString("MMMM")))
                .ToList();

            cbFilterMonth.ItemsSource = null;
            cbFilterMonth.ItemsSource = months;
            cbFilterMonth.DisplayMemberPath = "Name";
            cbFilterMonth.SelectedValuePath = "Value";
        }

        private void FillFilterYears()
        {
            var years = _attBLL.GetAll()
                .Where(a => a.AttendanceDate.HasValue)
                .Select(a => a.AttendanceDate!.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (years.Count == 0)
            {
                years.Add(DateTime.Now.Year);
            }

            cbFilterYear.ItemsSource = null;
            cbFilterYear.ItemsSource = years;
        }

        private void FillDataGridAttendance()
        {
            dgAttendance.ItemsSource = null;
            dgAttendance.ItemsSource = _attBLL.GetAll();
        }

        private void ApplyFilters()
        {
            IEnumerable<Attendance> data = _attBLL.GetAll();

            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(a =>
                    (a.Employee != null && a.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(a.Status) && a.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(a.DeviceIp) && a.DeviceIp.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (cbFilterEmployees.SelectedValue != null
                && int.TryParse(cbFilterEmployees.SelectedValue.ToString(), out int empId))
            {
                data = data.Where(a => a.EmployeeId == empId);
            }

            if (dpFilterDate.SelectedDate.HasValue)
            {
                DateOnly date = DateOnly.FromDateTime(dpFilterDate.SelectedDate.Value);
                data = data.Where(a => a.AttendanceDate == date);
            }

            if (cbFilterMonth.SelectedValue != null
                && int.TryParse(cbFilterMonth.SelectedValue.ToString(), out int month))
            {
                data = data.Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value.Month == month);
            }

            if (cbFilterYear.SelectedValue != null
                && int.TryParse(cbFilterYear.SelectedValue.ToString(), out int year))
            {
                data = data.Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value.Year == year);
            }

            dgAttendance.ItemsSource = null;
            dgAttendance.ItemsSource = data.ToList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbFilterEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void dpFilterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cbFilterEmployees.SelectedValue = null;
            dpFilterDate.SelectedDate = null;
            cbFilterMonth.SelectedValue = null;
            cbFilterYear.SelectedValue = null;
            txtSearch.Text = string.Empty;
            FillDataGridAttendance();
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
