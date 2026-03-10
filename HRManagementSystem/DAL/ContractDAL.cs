using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class ContractDAL: BaseDAL<Contract>
    {
        public override List<Contract> GetAll()
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Contracts
                              .Include(c => c.Employee)
                              .ToList();
            }
        }
    }
}
