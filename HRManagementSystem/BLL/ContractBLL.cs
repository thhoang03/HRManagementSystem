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

            string k = keyword.Trim();
            return _contDAL.GetAll()
                .Where(c =>
                    // search by employee name, contract type (full-time/part-time) or status
                    (c.Employee != null && !string.IsNullOrWhiteSpace(c.Employee.FullName) && c.Employee.FullName.Contains(k, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(c.ContractType) && c.ContractType.Contains(k, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(c.Status) && c.Status.Contains(k, StringComparison.OrdinalIgnoreCase))
                ).ToList();
        }
    }
}
