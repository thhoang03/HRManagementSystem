using System.Windows;

namespace HRManagementSystem.Views.Employee
{
    /// <summary>
    /// Interaction logic for MainEmployee.xaml
    /// </summary>
    public partial class MainEmployee : Window
    {
        public MainEmployee()
        {
            InitializeComponent();
        }

        private void MainEmployee_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CheckInOutView());
        }
    }
}

