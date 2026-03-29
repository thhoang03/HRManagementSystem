using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class PositionDAL: BaseDAL<Position>
    {
        public bool ExistsPositionName(string positionName, int? excludePositionId = null)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                return false;
            }

            string normalizedName = positionName.Trim().ToLower();

            using (var context = new HrmanagementSystemContext())
            {
                return context.Positions.Any(p =>
                    (excludePositionId == null || p.PositionId != excludePositionId.Value) &&
                    p.PositionName != null &&
                    p.PositionName.Trim().ToLower() == normalizedName);
            }
        }
    }
}
