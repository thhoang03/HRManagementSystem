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
            RefreshData();
        }

        private void RefreshData()
        {
            string keyword = txtSearch.Text.Trim();

            var pendingRequests = _leaveRequestBLL.GetAll()
                .Where(l => string.Equals(l.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                pendingRequests = pendingRequests.Where(l =>
                    (l.Employee != null && l.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.LeaveType) && l.LeaveType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Reason) && l.Reason.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            dgPendingLeaves.ItemsSource = null;
            dgPendingLeaves.ItemsSource = pendingRequests
                .OrderBy(l => l.StartDate)
                .ToList();

            ClearDetail();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshData();
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
            RefreshData();
        }

        private void ClearDetail()
        {
            txtEmployee.Text = "N/A";
            txtDateRange.Text = "N/A";
            txtLeaveType.Text = "N/A";
            txtReason.Text = "N/A";
        }
    }
}

