using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class UserBLL: BaseBLL<User>
    {
        private readonly UserDAL _userDAL;
        public UserBLL() : base(new UserDAL())
        {
            _userDAL = (UserDAL)_baseDAL;
        }
    }
}
