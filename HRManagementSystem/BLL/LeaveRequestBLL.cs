using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.BLL
{
    public class LeaveRequestBLL : BaseBLL<LeaveRequest>
    {
        private readonly LeaveRequestDAL _leaveDAL;

        public LeaveRequestBLL() : base(new LeaveRequestDAL())
        {
            _leaveDAL = (LeaveRequestDAL)_baseDAL;
        }

        public List<LeaveRequest> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _leaveDAL.GetAll().ToList();
            }

            return _leaveDAL.GetAll()
                .Where(l =>
                    (l.Employee != null && l.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.LeaveType) && l.LeaveType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Status) && l.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(l.Reason) && l.Reason.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
