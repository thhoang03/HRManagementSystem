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

        public List<User> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _userDAL.GetAll().ToList();
            }

            return _userDAL.GetAll()
                .Where(u =>
                    (!string.IsNullOrEmpty(u.Username) && u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(u.Role) && u.Role.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (u.Employee != null && u.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return _userDAL.GetAll()
                .FirstOrDefault(u =>
                    string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
                    && u.PasswordHash == password);
        }
    }
}
