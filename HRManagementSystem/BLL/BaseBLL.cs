using HRManagementSystem.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.BLL
{
    public class BaseBLL<T> where T:class
    {
        protected readonly BaseDAL<T> _baseDAL;

        public BaseBLL(BaseDAL<T> baseDAL)
        {
            _baseDAL = baseDAL;
        }

        public virtual List<T> GetAll() => _baseDAL.GetAll();
        public virtual void Add(T t) => _baseDAL.Add(t);   
        public virtual void Update(T t) => _baseDAL.Update(t);  
        public virtual void Delete(T t) => _baseDAL.Delete(t);
        
    }
}
