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

        public List<Employee> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return _empDAL.GetAll().ToList();

            string keyword = name.Trim();
            return _empDAL.GetAll()
                .Where(e =>
                    (!string.IsNullOrWhiteSpace(e.FullName) && e.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(e.Email) && e.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(e.Phone) && e.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                ).ToList();
        }
    }
}
