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
    /// Interaction logic for DepartmentsView.xaml
    /// </summary>
    public partial class DepartmentsView : Page
    {
        private DepartmentBLL _deptBLL = new();
        public DepartmentsView()
        {
            InitializeComponent();
        }

        private void FillDataGridDepartments()
        {
            dgDepartments.ItemsSource = null;
            dgDepartments.ItemsSource = _deptBLL.GetAll();
        }

        private void Clear()
        {
            txtDeptName.Text = string.Empty;
            txtDescription.Text = string.Empty;
        }

        private void frmDepartments_Loaded(object sender, RoutedEventArgs e)
        {
            FillDataGridDepartments();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtDeptName.Text.Trim();
            string des = txtDescription.Text.Trim();
            //validate

            //
            Department dept = new Department();
            dept.DepartmentName = name;
            dept.Description = des;
            _deptBLL.Add(dept);
            FillDataGridDepartments();
            Clear();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            txtDeptName.Focus();
        }

        private void dgDepartments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept != null)
            {
                txtDeptName.Text = dept.DepartmentName;
                txtDescription.Text = dept.Description;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept != null)
            {
                dept.DepartmentName = txtDeptName.Text.Trim();
                dept.Description = txtDescription.Text.Trim();

                _deptBLL.Update(dept);
                FillDataGridDepartments();
                Clear();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var dept = dgDepartments.SelectedItem as Department;
            if (dept != null)
            {
                if (MessageBox.Show("Do you really want to delete this department?",
                "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _deptBLL.Delete(dept);
                    FillDataGridDepartments();
                    Clear();
                }
            }

        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtSearch.Text.Trim();
            dgDepartments.ItemsSource = null;
            dgDepartments.ItemsSource = _deptBLL.Search(name);
        }
    }
}
