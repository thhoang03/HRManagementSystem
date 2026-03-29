using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class DepartmentDAL: BaseDAL<Department>
    {
        public bool ExistsDepartmentName(string departmentName, int? excludeDepartmentId = null)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                return false;
            }

            string normalizedName = departmentName.Trim().ToLower();

            using (var context = new HrmanagementSystemContext())
            {
                return context.Departments.Any(d =>
                    (excludeDepartmentId == null || d.DepartmentId != excludeDepartmentId.Value) &&
                    d.DepartmentName != null &&
                    d.DepartmentName.Trim().ToLower() == normalizedName);
            }
        }
    }
}
