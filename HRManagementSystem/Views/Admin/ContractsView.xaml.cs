using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for ContractsView.xaml
    /// </summary>
    public partial class ContractsView : Page
    {
        private readonly ContractBLL _contBLL = new();

        public ContractsView()
        {
            InitializeComponent();
        }

        private void frmContracts_Loaded(object sender, RoutedEventArgs e)
        {
            FillDgContracts();
        }

        private void FillDgContracts()
        {
            dgContracts.ItemsSource = null;
            dgContracts.ItemsSource = _contBLL.GetAll();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to deactivate this contract?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    contract.Status = "Deactive";
                    _contBLL.Update(contract);
                    FillDgContracts();
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to active this contract?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    contract.Status = "Active";
                    _contBLL.Update(contract);
                    FillDgContracts();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgContracts.ItemsSource = null;
            dgContracts.ItemsSource = _contBLL.Search(keyword);
        }
    }
}
