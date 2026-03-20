using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Page
    {
        private readonly SettingBLL _settingBLL = new();

        public SettingsView()
        {
            InitializeComponent();
        }

        private void frmSettings_Loaded(object sender, RoutedEventArgs e)
        {
            FillDgSettings();
        }

        private void FillDgSettings()
        {
            dgSettings.ItemsSource = null;
            dgSettings.ItemsSource = _settingBLL.GetAll();
        }

        private void Clear()
        {
            txtKey.Text = string.Empty;
            txtValue.Text = string.Empty;
            txtDescription.Text = string.Empty;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtKey.Focus();
        }

        private void dgSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var setting = dgSettings.SelectedItem as Setting;
            if (setting != null)
            {
                txtKey.Text = setting.SettingKey;
                txtValue.Text = setting.SettingValue;
                txtDescription.Text = setting.Description ?? string.Empty;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string key = txtKey.Text.Trim();
            string value = txtValue.Text.Trim();

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            Setting setting = new();
            setting.SettingKey = key;
            setting.SettingValue = value;
            setting.Description = txtDescription.Text.Trim();

            _settingBLL.Add(setting);
            FillDgSettings();
            Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var setting = dgSettings.SelectedItem as Setting;
            if (setting != null)
            {
                string key = txtKey.Text.Trim();
                string value = txtValue.Text.Trim();

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                setting.SettingKey = key;
                setting.SettingValue = value;
                setting.Description = txtDescription.Text.Trim();

                _settingBLL.Update(setting);
                FillDgSettings();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var setting = dgSettings.SelectedItem as Setting;
            if (setting != null)
            {
                MessageBox.Show("Setting does not support Status. Delete action is disabled.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            dgSettings.ItemsSource = null;
            dgSettings.ItemsSource = _settingBLL.Search(keyword);
        }
    }
}
