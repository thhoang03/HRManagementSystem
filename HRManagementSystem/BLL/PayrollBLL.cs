using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.BLL
{
    public class PayrollBLL : BaseBLL<Payroll>
    {
        private readonly PayrollDAL _payDAL;

        public PayrollBLL() : base(new PayrollDAL())
        {
            _payDAL = (PayrollDAL)_baseDAL;
        }

        public List<Payroll> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _payDAL.GetAll().ToList();
            }

            return _payDAL.GetAll()
                .Where(p =>
                    (p.Employee != null && p.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(p.Status) && p.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (p.Contract != null && !string.IsNullOrEmpty(p.Contract.ContractType)
                        && p.Contract.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
