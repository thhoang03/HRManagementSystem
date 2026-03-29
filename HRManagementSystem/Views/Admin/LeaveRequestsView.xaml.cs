using HRManagementSystem.BLL;
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
            FillDgLeaveRequests();
        }

        private void FillDgLeaveRequests()
        {
            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = _leaveBLL.GetAll();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgLeaveRequests.ItemsSource = null;
            dgLeaveRequests.ItemsSource = _leaveBLL.Search(keyword);
        }
    }
}
