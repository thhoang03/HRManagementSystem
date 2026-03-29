using System.Windows;

namespace HRManagementSystem.Views.HR
{
    /// <summary>
    /// Interaction logic for MainHR.xaml
    /// </summary>
    public partial class MainHR : Window
    {
        public MainHR()
        {
            InitializeComponent();
        }

        private void MainHR_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Employee());
        }
    }
}
