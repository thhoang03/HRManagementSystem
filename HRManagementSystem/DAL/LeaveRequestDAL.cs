using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.DAL
{
    public class LeaveRequestDAL : BaseDAL<LeaveRequest>
    {
        public override List<LeaveRequest> GetAll()
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.ApprovedByNavigation)
                    .ToList();
            }
        }
    }
}
