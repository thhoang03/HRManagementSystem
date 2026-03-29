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
            try
            {
                string name = txtPositionName.Text.Trim();
                string baseSalary_str = txtBaseSalary.Text.Trim();
                if (!TryGetPositionInput(name, baseSalary_str, null, out decimal baseSalary))
                {
                    return;
                }

                Position p = new();
                p.PositionName = name;
                p.BaseSalary = baseSalary;
                p.Status = "Active";

                _posBLL.Add(p);
                FillDataGridPositons();
                Clear();
            }
            catch (Exception ex)
            {
                ShowPositionSaveError(ex);
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
                try
                {
                    string name = txtPositionName.Text.Trim();
                    string baseSalary_str = txtBaseSalary.Text.Trim();
                    if (!TryGetPositionInput(name, baseSalary_str, pos.PositionId, out decimal baseSalary))
                    {
                        return;
                    }

                    pos.PositionName = name;
                    pos.BaseSalary = baseSalary;

                    _posBLL.Update(pos);
                    FillDataGridPositons();
                    Clear();
                }
                catch (Exception ex)
                {
                    ShowPositionSaveError(ex);
                }
            }
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Active");
        }

        private void btnDeactive_Click(object sender, RoutedEventArgs e)
        {
            UpdateSelectedStatus("Deactive");
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtSearch.Text.Trim();
            dgPositions.ItemsSource = null;
            dgPositions.ItemsSource = _posBLL.Search(name);
        }

        private bool TryGetPositionInput(string name, string baseSalaryText, int? excludePositionId, out decimal baseSalary)
        {
            baseSalary = 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Position name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPositionName.Focus();
                return false;
            }

            if (!_posBLL.IsPositionNameUnique(name, excludePositionId))
            {
                MessageBox.Show("Position name already exists. Please choose a different name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPositionName.Focus();
                return false;
            }

            if (!decimal.TryParse(baseSalaryText, out baseSalary))
            {
                MessageBox.Show("Base salary must be a valid number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBaseSalary.Focus();
                return false;
            }

            if (baseSalary <= 0)
            {
                MessageBox.Show("Base salary must be greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBaseSalary.Focus();
                return false;
            }

            return true;
        }

        private void UpdateSelectedStatus(string status)
        {
            var pos = dgPositions.SelectedItem as Position;
            if (pos == null)
            {
                MessageBox.Show("Please select a position first.", "Position", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            pos.Status = status;
            try
            {
                _posBLL.Update(pos);
                FillDataGridPositons();
            }
            catch (Exception ex)
            {
                ShowPositionSaveError(ex);
            }
        }

        private void ShowPositionSaveError(Exception ex)
        {
            string message = ex.Message ?? string.Empty;
            if (IsUniqueConstraintViolation(message))
            {
                MessageBox.Show("Position name already exists. Please choose a different name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPositionName.Focus();
                return;
            }

            MessageBox.Show($"Cannot save position. {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static bool IsUniqueConstraintViolation(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            return errorMessage.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
