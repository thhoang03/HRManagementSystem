using System.Windows;

namespace HRManagementSystem.Views.Manager
{
    /// <summary>
    /// Interaction logic for MainManager.xaml
    /// </summary>
    public partial class MainManager : Window
    {
        public MainManager()
        {
            InitializeComponent();
        }

        private void MainManager_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApproveLeaveView());
        }
    }
}

