using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public  class PositionBLL: BaseBLL<Position>
    {
        private readonly PositionDAL _posDAL;
        public PositionBLL(): base(new PositionDAL())
        {
            _posDAL = (PositionDAL) _baseDAL;
        }
    }
}
