using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class AttendanceBLL : BaseBLL<Attendance>
    {
        private readonly AttendanceDAL _attDAL;

        public AttendanceBLL() : base(new AttendanceDAL())
        {
            _attDAL = (AttendanceDAL)_baseDAL;
        }

        public List<Attendance> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _attDAL.GetAll().ToList();
            }

            return _attDAL.GetAll()
                .Where(a =>
                    (a.Employee != null && a.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(a.Status) && a.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(a.DeviceIp) && a.DeviceIp.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
