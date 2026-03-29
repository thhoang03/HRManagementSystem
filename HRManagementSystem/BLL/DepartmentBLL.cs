using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class DepartmentBLL: BaseBLL<Department>
    {
        private readonly DepartmentDAL _deptDAL;
        public DepartmentBLL() : base(new DepartmentDAL())
        {
            _deptDAL = (DepartmentDAL)_baseDAL;
        }
     
        public List<Department> Search(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _deptDAL.GetAll().ToList();
            }

            return _deptDAL.GetAll()
                           .Where(d => d.DepartmentName.Contains(name, StringComparison.OrdinalIgnoreCase)
                                    || (d.Description ?? string.Empty).Contains(name, StringComparison.OrdinalIgnoreCase)
                                    || (!string.IsNullOrEmpty(d.Status) && d.Status.Contains(name, StringComparison.OrdinalIgnoreCase)))
                           .ToList();
        }

    }
}
