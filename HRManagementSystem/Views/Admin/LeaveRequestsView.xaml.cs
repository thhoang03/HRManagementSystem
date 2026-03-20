using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for LeaveRequestsView.xaml
    /// </summary>
    public partial class LeaveRequestsView : Page
    {
        private readonly LeaveRequestBLL _leaveBLL = new();
        private readonly EmployeeBLL _empBLL = new();

        public LeaveRequestsView()
        {
            InitializeComponent();
        }

        private void frmLeaveRequests_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            FillComboApprovedBy();
            FillDgLeaveRequests();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        private void FillComboApprovedBy()
        {
            cbApprovedBy.ItemsSource = null;
            cbApprovedBy.ItemsSource = _empBLL.GetAll();
            cbApprovedBy.SelectedValuePath = "EmployeeId";
            cbApprovedBy.DisplayMemberPath = "FullName";
        }

        private void FillDgLeaveRequests()
        {
            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = _leaveBLL.GetAll();
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtLeaveType.Text = string.Empty;
            txtReason.Text = string.Empty;
            cbStatus.SelectedIndex = -1;
            cbApprovedBy.SelectedValue = null;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            cbEmployees.Focus();
        }

        private void dgLeaveRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var leave = dgLeaveRequests.SelectedItem as LeaveRequest;
            if (leave != null)
            {
                cbEmployees.SelectedValue = leave.EmployeeId;
                dpStartDate.SelectedDate = leave.StartDate;
                dpEndDate.SelectedDate = leave.EndDate;
                txtLeaveType.Text = leave.LeaveType ?? string.Empty;
                txtReason.Text = leave.Reason ?? string.Empty;
                cbStatus.Text = leave.Status ?? string.Empty;
                cbApprovedBy.SelectedValue = leave.ApprovedBy;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetLeaveRequestInput(out LeaveRequestInput input))
            {
                return;
            }

            LeaveRequest leave = new();
            leave.EmployeeId = input.EmployeeId;
            leave.StartDate = input.StartDate;
            leave.EndDate = input.EndDate;
            leave.LeaveType = input.LeaveType;
            leave.Reason = input.Reason;
            leave.Status = input.Status;
            leave.ApprovedBy = input.ApprovedBy;

            _leaveBLL.Add(leave);
            FillDgLeaveRequests();
            Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var leave = dgLeaveRequests.SelectedItem as LeaveRequest;
            if (leave != null)
            {
                if (!TryGetLeaveRequestInput(out LeaveRequestInput input))
                {
                    return;
                }

                leave.EmployeeId = input.EmployeeId;
                leave.StartDate = input.StartDate;
                leave.EndDate = input.EndDate;
                leave.LeaveType = input.LeaveType;
                leave.Reason = input.Reason;
                leave.Status = input.Status;
                leave.ApprovedBy = input.ApprovedBy;

                _leaveBLL.Update(leave);
                FillDgLeaveRequests();
                Clear();
            }
        }   

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = _leaveBLL.Search(keyword);
        }

        private bool TryGetLeaveRequestInput(out LeaveRequestInput input)
        {
            input = new LeaveRequestInput();

            if (cbEmployees.SelectedValue == null || !int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId))
            {
                MessageBox.Show("Please select a valid employee.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbEmployees.Focus();
                return false;
            }

            if (!dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Start date and end date are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return false;
            }

            DateTime startDate = dpStartDate.SelectedDate.Value;
            DateTime endDate = dpEndDate.SelectedDate.Value;
            if (endDate.Date < startDate.Date)
            {
                MessageBox.Show("End date cannot be earlier than start date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpEndDate.Focus();
                return false;
            }

            string leaveType = txtLeaveType.Text.Trim();
            if (string.IsNullOrWhiteSpace(leaveType))
            {
                MessageBox.Show("Leave type is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLeaveType.Focus();
                return false;
            }

            string status = cbStatus.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show("Status is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbStatus.Focus();
                return false;
            }

            int? approvedBy = null;
            if (cbApprovedBy.SelectedValue != null)
            {
                if (!int.TryParse(cbApprovedBy.SelectedValue.ToString(), out int approvedById))
                {
                    MessageBox.Show("Approved By is invalid.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbApprovedBy.Focus();
                    return false;
                }
                approvedBy = approvedById;
            }

            input = new LeaveRequestInput
            {
                EmployeeId = empId,
                StartDate = startDate,
                EndDate = endDate,
                LeaveType = leaveType,
                Reason = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text.Trim(),
                Status = status,
                ApprovedBy = approvedBy
            };
            return true;
        }

        private sealed class LeaveRequestInput
        {
            public int EmployeeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? LeaveType { get; set; }
            public string? Reason { get; set; }
            public string? Status { get; set; }
            public int? ApprovedBy { get; set; }
        }
    }
}
