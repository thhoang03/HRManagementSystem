using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
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

        public MyLeaveRequestsView()
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
            if (!TryGetCurrentEmployeeId(out int employeeId))
            {
                return;
            }

            var myLeaves = _leaveRequestBLL.GetAll()
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToList();

            dgMyLeaves.ItemsSource = null;
            dgMyLeaves.ItemsSource = myLeaves;
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

