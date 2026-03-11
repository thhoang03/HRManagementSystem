using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for ContractsView.xaml
    /// </summary>
    public partial class ContractsView : Page
    {
        private readonly ContractBLL _contBLL = new();
        private readonly EmployeeBLL _empBLL = new();

        public ContractsView()
        {
            InitializeComponent();
        }

        private void frmContracts_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboEmployees();
            FillDgContracts();
        }

        private void FillDgContracts()
        {
            dgContracts.ItemsSource = null;
            dgContracts.ItemsSource = _contBLL.GetAll();
        }

        private void FillComboEmployees()
        {
            cbEmployees.ItemsSource = null;
            cbEmployees.ItemsSource = _empBLL.GetAll();
            cbEmployees.SelectedValuePath = "EmployeeId";
            cbEmployees.DisplayMemberPath = "FullName";
        }

        private void Clear()
        {
            cbEmployees.SelectedValue = null;
            txtContractType.Text = string.Empty;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            txtSalary.Text = string.Empty;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            cbEmployees.Focus();
        }

        private void dgContracts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                cbEmployees.SelectedValue = contract.EmployeeId;
                txtContractType.Text = contract.ContractType;
                dpStartDate.SelectedDate = contract.StartDate.ToDateTime(TimeOnly.MinValue);
                dpEndDate.SelectedDate = contract.EndDate?.ToDateTime(TimeOnly.MinValue);
                txtSalary.Text = contract.ContractSalary?.ToString();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (cbEmployees.SelectedValue == null || !dpStartDate.SelectedDate.HasValue)
                {
                    return;
                }

                string? empIdStr = cbEmployees.SelectedValue.ToString();
                string contractType = txtContractType.Text.Trim();
                DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                DateOnly? endDate = null;
                if (dpEndDate.SelectedDate.HasValue)
                {
                    endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                }

                decimal? salary = null;
                if (decimal.TryParse(txtSalary.Text.Trim(), out decimal salaryValue))
                {
                    salary = salaryValue;
                }

                int.TryParse(empIdStr, out int empId);

                contract.EmployeeId = empId;
                contract.ContractType = contractType;
                contract.StartDate = startDate;
                contract.EndDate = endDate;
                contract.ContractSalary = salary;

                _contBLL.Update(contract);
                FillDgContracts();
                Clear();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cbEmployees.SelectedValue == null || !dpStartDate.SelectedDate.HasValue)
            {
                return;
            }

            string? empIdStr = cbEmployees.SelectedValue.ToString();
            string contractType = txtContractType.Text.Trim();
            DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
            DateOnly? endDate = null;
            if (dpEndDate.SelectedDate.HasValue)
            {
                endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
            }

            decimal? salary = null;
            if (decimal.TryParse(txtSalary.Text.Trim(), out decimal salaryValue))
            {
                salary = salaryValue;
            }

            int.TryParse(empIdStr, out int empId);

            Contract contract = new();
            contract.EmployeeId = empId;
            contract.ContractType = contractType;
            contract.StartDate = startDate;
            contract.EndDate = endDate;
            contract.ContractSalary = salary;

            _contBLL.Add(contract);
            FillDgContracts();
            Clear();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var contract = dgContracts.SelectedItem as Contract;
            if (contract != null)
            {
                if (MessageBox.Show("Do you really want to delete this contract?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _contBLL.Delete(contract);
                    FillDgContracts();
                    Clear();
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
