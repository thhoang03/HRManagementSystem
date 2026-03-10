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

        private void btnSave_Click(object sender, RoutedEventArgs e)
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

        
    }
}
