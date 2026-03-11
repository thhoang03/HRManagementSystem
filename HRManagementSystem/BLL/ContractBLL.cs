using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class ContractBLL:BaseBLL<Contract>
    {
        private readonly ContractDAL _contDAL;
        public ContractBLL() :base(new ContractDAL())
        {
            _contDAL = (ContractDAL)_baseDAL;
        }

        public List<Contract> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _contDAL.GetAll().ToList();
            }

            return _contDAL.GetAll()
                .Where(c =>
                    (c.Employee != null && c.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(c.ContractType) && c.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(c.Status) && c.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
