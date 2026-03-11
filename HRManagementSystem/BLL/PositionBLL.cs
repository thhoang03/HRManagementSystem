using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using Microsoft.IdentityModel.Tokens;
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

        public List<Position> Search(string name)
        {
            if(name.IsNullOrEmpty())
            {
                return _posDAL.GetAll();
            }
            return _posDAL.GetAll()
                          .Where(p => p.PositionName.ToLower().Contains(name.ToLower()))
                          .ToList();
        }
    }
}
