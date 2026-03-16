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
            if (cbEmployees.SelectedValue == null || !dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
            {
                return;
            }

            int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId);

            int? approvedBy = null;
            if (cbApprovedBy.SelectedValue != null
                && int.TryParse(cbApprovedBy.SelectedValue.ToString(), out int approvedId))
            {
                approvedBy = approvedId;
            }

            LeaveRequest leave = new();
            leave.EmployeeId = empId;
            leave.StartDate = dpStartDate.SelectedDate.Value;
            leave.EndDate = dpEndDate.SelectedDate.Value;
            leave.LeaveType = txtLeaveType.Text.Trim();
            leave.Reason = txtReason.Text.Trim();
            leave.Status = cbStatus.Text?.Trim();
            leave.ApprovedBy = approvedBy;

            _leaveBLL.Add(leave);
            FillDgLeaveRequests();
            Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var leave = dgLeaveRequests.SelectedItem as LeaveRequest;
            if (leave != null)
            {
                if (cbEmployees.SelectedValue == null || !dpStartDate.SelectedDate.HasValue || !dpEndDate.SelectedDate.HasValue)
                {
                    return;
                }

                int.TryParse(cbEmployees.SelectedValue.ToString(), out int empId);

                int? approvedBy = null;
                if (cbApprovedBy.SelectedValue != null
                    && int.TryParse(cbApprovedBy.SelectedValue.ToString(), out int approvedId))
                {
                    approvedBy = approvedId;
                }

                leave.EmployeeId = empId;
                leave.StartDate = dpStartDate.SelectedDate.Value;
                leave.EndDate = dpEndDate.SelectedDate.Value;
                leave.LeaveType = txtLeaveType.Text.Trim();
                leave.Reason = txtReason.Text.Trim();
                leave.Status = cbStatus.Text?.Trim();
                leave.ApprovedBy = approvedBy;

                _leaveBLL.Update(leave);
                FillDgLeaveRequests();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var leave = dgLeaveRequests.SelectedItem as LeaveRequest;
            if (leave != null)
            {
                if (MessageBox.Show("Do you really want to delete this leave request?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _leaveBLL.Delete(leave);
                    FillDgLeaveRequests();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = _leaveBLL.Search(keyword);
        }
    }
}
