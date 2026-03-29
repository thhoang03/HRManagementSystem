using HRManagementSystem.BLL;
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

        public LeaveRequestsView()
        {
            InitializeComponent();
        }

        private void frmLeaveRequests_Loaded(object sender, RoutedEventArgs e)
        {
            cbFilterStatus.SelectedIndex = 0;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string keyword = txtSearch.Text.Trim();
            string status = (cbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            DateTime? fromDate = dpFromDate.SelectedDate?.Date;
            DateTime? toDate = dpToDate.SelectedDate?.Date;

            var leaves = _leaveBLL.GetAll().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                leaves = leaves.Where(l =>
                    (l.Employee != null && l.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.LeaveType) && l.LeaveType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Status) && l.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Reason) && l.Reason.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                leaves = leaves.Where(l => string.Equals(l.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                leaves = leaves.Where(l => l.StartDate.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                leaves = leaves.Where(l => l.EndDate.Date <= toDate.Value);
            }

            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = leaves
                .OrderByDescending(l => l.StartDate)
                .ToList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
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
    }
}
