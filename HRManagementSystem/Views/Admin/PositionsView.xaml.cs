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
    /// Interaction logic for PositionsView.xaml
    /// </summary>
    public partial class PositionsView : Page
    {
        private PositionBLL _posBLL = new();
        public PositionsView()
        {
            InitializeComponent();
        }

        private void Clear()
        {
            txtPositionName.Text = string.Empty;
            txtBaseSalary.Text = string.Empty;
        }

        private void FillDataGridPositons()
        {
            dgPositions.ItemsSource = null;
            dgPositions.ItemsSource = _posBLL.GetAll();
        }

        private void frmPositions_Loaded(object sender, RoutedEventArgs e)
        {
            FillDataGridPositons();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtPositionName.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtPositionName.Text.Trim();
            string baseSalary_str = txtBaseSalary.Text.Trim();
            decimal.TryParse(baseSalary_str, out decimal baseSalary);
            if (baseSalary > 0) 
            {
                Position p = new();
                p.PositionName = name;
                p.BaseSalary = baseSalary;

                _posBLL.Add(p);
                FillDataGridPositons();
                Clear();
            }
        }

        private void dgPositions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pos = dgPositions.SelectedItem as Position;
            if (pos != null)
            {
                txtPositionName.Text = pos.PositionName;
                txtBaseSalary.Text = pos.BaseSalary.ToString();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var pos = dgPositions.SelectedItem as Position;
            if (pos != null)
            {
                string name = txtPositionName.Text.Trim();
                string baseSalary_str = txtBaseSalary.Text.Trim();
                decimal.TryParse(baseSalary_str, out decimal baseSalary);
                if (baseSalary > 0)
                {
                    pos.PositionName = name;
                    pos.BaseSalary = baseSalary;

                    _posBLL.Update(pos);
                    FillDataGridPositons();
                    Clear();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var pos = dgPositions.SelectedItem as Position;
            if (pos != null)
            {
                if(MessageBox.Show("Do you really want to delete this position?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning)==MessageBoxResult.Yes)
                {
                    _posBLL.Delete(pos);
                    FillDataGridPositons();
                    Clear();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtSearch.Text.Trim();
            dgPositions.ItemsSource = null;
            dgPositions.ItemsSource = _posBLL.Search(name);
        }
    }
}
