using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class EmployeeBLL: BaseBLL<Employee>
    {
        private readonly EmployeeDAL _empDAL;

        public EmployeeBLL() : base(new EmployeeDAL())
        {
            _empDAL = (EmployeeDAL)_baseDAL;
        }
    }
}
