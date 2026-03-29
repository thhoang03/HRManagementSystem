using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Manager
{
    /// <summary>
    /// Interaction logic for ApproveLeaveView.xaml
    /// </summary>
    public partial class ApproveLeaveView : Page
    {
        private readonly LeaveRequestBLL _leaveRequestBLL = new();
        private User? _currentUser;

        public ApproveLeaveView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUser = Application.Current.Properties["CurrentUser"] as User;
            LoadFilterOptions();
            ApplyFilters();
        }

        private void LoadFilterOptions()
        {
            cbStatus.ItemsSource = new[] { "Pending", "Approved", "Rejected", "All" };
            cbStatus.SelectedItem = "Pending";

            var leaveTypes = _leaveRequestBLL.GetAll()
                .Where(l => !string.IsNullOrWhiteSpace(l.LeaveType))
                .Select(l => l.LeaveType!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();

            leaveTypes.Insert(0, "All");
            cbLeaveType.ItemsSource = leaveTypes;
            cbLeaveType.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            string keyword = txtSearch.Text.Trim();
            string status = cbStatus.SelectedItem as string ?? "Pending";
            string leaveType = cbLeaveType.SelectedItem as string ?? "All";
            DateOnly? fromDate = dpFromDate.SelectedDate.HasValue ? DateOnly.FromDateTime(dpFromDate.SelectedDate.Value) : null;
            DateOnly? toDate = dpToDate.SelectedDate.HasValue ? DateOnly.FromDateTime(dpToDate.SelectedDate.Value) : null;

            IEnumerable<LeaveRequest> requests = _leaveRequestBLL.GetAll();

            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(l => string.Equals(l.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(leaveType, "All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(l => string.Equals(l.LeaveType, leaveType, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                requests = requests.Where(l => DateOnly.FromDateTime(l.StartDate) >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                requests = requests.Where(l => DateOnly.FromDateTime(l.EndDate) <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                requests = requests.Where(l =>
                    (l.Employee != null && l.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.LeaveType) && l.LeaveType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Reason) && l.Reason.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            dgPendingLeaves.ItemsSource = null;
            dgPendingLeaves.ItemsSource = requests
                .OrderBy(l => l.StartDate)
                .ToList();

            ClearDetail();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void cbLeaveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            cbStatus.SelectedItem = "Pending";
            cbLeaveType.SelectedIndex = 0;
            dpFromDate.SelectedDate = null;
            dpToDate.SelectedDate = null;
            ApplyFilters();
        }

        private void dgPendingLeaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPendingLeaves.SelectedItem is not LeaveRequest selected)
            {
                ClearDetail();
                return;
            }

            txtEmployee.Text = selected.Employee?.FullName ?? "N/A";
            txtDateRange.Text = $"{selected.StartDate:dd/MM/yyyy} - {selected.EndDate:dd/MM/yyyy}";
            txtLeaveType.Text = string.IsNullOrWhiteSpace(selected.LeaveType) ? "N/A" : selected.LeaveType;
            txtReason.Text = string.IsNullOrWhiteSpace(selected.Reason) ? "N/A" : selected.Reason;

            bool canProcess = string.Equals(selected.Status, "Pending", StringComparison.OrdinalIgnoreCase);
            btnApprove.IsEnabled = canProcess;
            btnReject.IsEnabled = canProcess;
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Approved");
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Rejected");
        }

        private void UpdateSelectedStatus(string newStatus)
        {
            if (dgPendingLeaves.SelectedItem is not LeaveRequest selected)
            {
                MessageBox.Show("Please select a leave request first.", "Approval", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentUser?.EmployeeId == null)
            {
                MessageBox.Show("Current manager account is not linked with an employee.", "Approval", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selected.Status = newStatus;
            selected.ApprovedBy = _currentUser.EmployeeId.Value;
            _leaveRequestBLL.Update(selected);

            MessageBox.Show($"Request has been {newStatus.ToLowerInvariant()}.", "Approval", MessageBoxButton.OK, MessageBoxImage.Information);
            ApplyFilters();
        }

        private void ClearDetail()
        {
            txtEmployee.Text = "N/A";
            txtDateRange.Text = "N/A";
            txtLeaveType.Text = "N/A";
            txtReason.Text = "N/A";
            btnApprove.IsEnabled = false;
            btnReject.IsEnabled = false;
        }
    }
}

