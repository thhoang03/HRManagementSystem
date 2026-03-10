using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class EmployeeDAL: BaseDAL<Employee>
    {
        public override List<Employee> GetAll()
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Employees
                              .Include(e => e.Department)
                              .Include(e => e.Position)
                              .Include(e => e.User)
                              .ToList();
            }
        }
    }
}
