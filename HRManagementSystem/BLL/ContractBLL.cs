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
    }
}
