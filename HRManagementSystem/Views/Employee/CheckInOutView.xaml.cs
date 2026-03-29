using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Employee
{
    /// <summary>
    /// Interaction logic for CheckInOutView.xaml
    /// </summary>
    public partial class CheckInOutView : Page
    {
        private readonly AttendanceBLL _attendanceBLL = new();
        private User? _currentUser;
        private List<Attendance> _myAttendance = new();

        public CheckInOutView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUser = Application.Current.Properties["CurrentUser"] as User;
            txtToday.Text = $"Today: {DateTime.Now:dddd, dd MMM yyyy}";
            cbFilterStatus.SelectedIndex = 0;
            RefreshData();
        }

        private void RefreshData()
        {
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            _myAttendance = _attendanceBLL.GetAll()
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AttendanceDate)
                .ThenByDescending(a => a.CheckIn)
                .ToList();

            ApplyFilters();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayRecord = _myAttendance.FirstOrDefault(a => a.AttendanceDate == today);

            if (todayRecord == null)
            {
                txtStatus.Text = "You have not checked in today.";
                btnCheckIn.IsEnabled = true;
                btnCheckOut.IsEnabled = false;
                return;
            }

            string checkInText = todayRecord.CheckIn?.ToString("g") ?? "N/A";
            string checkOutText = todayRecord.CheckOut?.ToString("g") ?? "N/A";
            txtStatus.Text = $"Check in: {checkInText} | Check out: {checkOutText} | Status: {todayRecord.Status}";

            btnCheckIn.IsEnabled = !todayRecord.CheckIn.HasValue;
            btnCheckOut.IsEnabled = todayRecord.CheckIn.HasValue && !todayRecord.CheckOut.HasValue;
        }

        private void btnCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayRecord = _attendanceBLL.GetAll()
                .FirstOrDefault(a => a.EmployeeId == employeeId && a.AttendanceDate == today);

            if (todayRecord != null && todayRecord.CheckIn.HasValue)
            {
                MessageBox.Show("You already checked in today.", "Attendance", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
                return;
            }

            if (todayRecord == null)
            {
                todayRecord = new Attendance
                {
                    EmployeeId = employeeId,
                    AttendanceDate = today,
                    CheckIn = DateTime.Now,
                    Status = "Present",
                    DeviceIp = "Manual"
                };
                _attendanceBLL.Add(todayRecord);
            }
            else
            {
                todayRecord.CheckIn = DateTime.Now;
                todayRecord.Status = "Present";
                if (string.IsNullOrWhiteSpace(todayRecord.DeviceIp))
                {
                    todayRecord.DeviceIp = "Manual";
                }
                _attendanceBLL.Update(todayRecord);
            }

            MessageBox.Show("Check in successful.", "Attendance", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshData();
        }

        private void btnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayRecord = _attendanceBLL.GetAll()
                .FirstOrDefault(a => a.EmployeeId == employeeId && a.AttendanceDate == today);

            if (todayRecord == null || !todayRecord.CheckIn.HasValue)
            {
                MessageBox.Show("You need to check in first.", "Attendance", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshData();
                return;
            }

            if (todayRecord.CheckOut.HasValue)
            {
                MessageBox.Show("You already checked out today.", "Attendance", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshData();
                return;
            }

            todayRecord.CheckOut = DateTime.Now;
            _attendanceBLL.Update(todayRecord);
            MessageBox.Show("Check out successful.", "Attendance", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshData();
        }

        private void ApplyFilters()
        {
            IEnumerable<Attendance> data = _myAttendance;

            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(a =>
                    (!string.IsNullOrEmpty(a.Status) && a.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(a.DeviceIp) && a.DeviceIp.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            string status = (cbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                data = data.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            DateOnly? fromDate = dpFromDate.SelectedDate.HasValue
                ? DateOnly.FromDateTime(dpFromDate.SelectedDate.Value)
                : null;
            DateOnly? toDate = dpToDate.SelectedDate.HasValue
                ? DateOnly.FromDateTime(dpToDate.SelectedDate.Value)
                : null;

            if (fromDate.HasValue)
            {
                data = data.Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                data = data.Where(a => a.AttendanceDate.HasValue && a.AttendanceDate.Value <= toDate.Value);
            }

            dgAttendance.ItemsSource = null;
            dgAttendance.ItemsSource = data.ToList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void cbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void dpFromDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void dpToDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            cbFilterStatus.SelectedIndex = 0;
            dpFromDate.SelectedDate = null;
            dpToDate.SelectedDate = null;
            ApplyFilters();
        }

        private bool TryGetCurrentEmployeeId(out int employeeId)
        {
            employeeId = 0;
            if (_currentUser?.EmployeeId == null)
            {
                MessageBox.Show("Current account is not linked with an employee.", "Access", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            employeeId = _currentUser.EmployeeId.Value;
            return true;
        }
    }
}

