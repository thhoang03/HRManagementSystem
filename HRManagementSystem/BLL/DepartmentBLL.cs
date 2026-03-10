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
     

    }
}
