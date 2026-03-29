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
    /// Interaction logic for MyLeaveRequestsView.xaml
    /// </summary>
    public partial class MyLeaveRequestsView : Page
    {
        private readonly LeaveRequestBLL _leaveRequestBLL = new();
        private User? _currentUser;
        private List<LeaveRequest> _myLeaves = new();

        public MyLeaveRequestsView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUser = Application.Current.Properties["CurrentUser"] as User;
            cbFilterStatus.SelectedIndex = 0;
            RefreshData();
        }

        private void RefreshData()
        {
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            _myLeaves = _leaveRequestBLL.GetAll()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToList();

            ApplyFilters();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select start date and end date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = dpStartDate.SelectedDate.Value.Date;
            DateTime endDate = dpEndDate.SelectedDate.Value.Date;
            if (endDate < startDate)
            {
                MessageBox.Show("End date cannot be earlier than start date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string leaveType = txtLeaveType.Text.Trim();
            if (string.IsNullOrWhiteSpace(leaveType))
            {
                MessageBox.Show("Leave type is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                LeaveType = leaveType,
                Reason = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text.Trim(),
                Status = "Pending",
                ApprovedBy = null
            };

            _leaveRequestBLL.Add(newRequest);
            MessageBox.Show("Leave request submitted.", "Leave Request", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            RefreshData();
        }

        private void ClearForm()
        {
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtLeaveType.Text = string.Empty;
            txtReason.Text = string.Empty;
        }

        private void ApplyFilters()
        {
            IEnumerable<LeaveRequest> data = _myLeaves;

            string keyword = txtSearch.Text.Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                data = data.Where(l =>
                    (!string.IsNullOrEmpty(l.LeaveType) && l.LeaveType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Status) && l.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Reason) && l.Reason.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            string status = (cbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                data = data.Where(l => string.Equals(l.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            DateTime? fromDate = dpFromDate.SelectedDate?.Date;
            DateTime? toDate = dpToDate.SelectedDate?.Date;

            if (fromDate.HasValue)
            {
                data = data.Where(l => l.StartDate.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                data = data.Where(l => l.EndDate.Date <= toDate.Value);
            }

            dgMyLeaves.ItemsSource = null;
            dgMyLeaves.ItemsSource = data
                .OrderByDescending(l => l.StartDate)
                .ToList();
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

