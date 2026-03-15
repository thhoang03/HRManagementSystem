using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class AttendanceDAL : BaseDAL<Attendance>
    {
        public override List<Attendance> GetAll()
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Attendances
                              .Include(a => a.Employee)
                              .ToList();
            }
        }
    }
}
