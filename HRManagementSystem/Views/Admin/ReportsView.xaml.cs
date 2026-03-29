using HRManagementSystem.Views.Admin.Reports;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Admin
{
    public partial class ReportsView : Page
    {
        public ReportsView()
        {
            InitializeComponent();
            Loaded += ReportsView_Loaded;
        }

        private void ReportsView_Loaded(object sender, RoutedEventArgs e)
        {
            frameAttendance.Navigate(new AttendanceReportView());
            frameLeave.Navigate(new LeaveReportView());
            framePayroll.Navigate(new PayrollReportView());
            frameContract.Navigate(new ContractReportView());
        }
    }
}
