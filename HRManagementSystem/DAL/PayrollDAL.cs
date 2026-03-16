using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.DAL
{
    public class PayrollDAL : BaseDAL<Payroll>
    {
        public override List<Payroll> GetAll()
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Payrolls
                              .Include(p => p.Employee)
                              .Include(p => p.Contract)
                              .ToList();
            }
        }
    }
}
