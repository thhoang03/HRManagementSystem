using HRManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.DAL
{
    public class BaseDAL<T> where T: class
    {
        public virtual List<T> GetAll()
        {
            try
            {
                using (var context = new HrmanagementSystemContext())
                {
                    return context.Set<T>().ToList();
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual void Add(T t)
        {
            try
            {
                using (var context = new HrmanagementSystemContext())
                {
                    context.Set<T>().Add(t);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual void Update(T t)
        {
            try
            {
                using (var context = new HrmanagementSystemContext())
                {
                    context.Set<T>().Update(t);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual void Delete(T t)
        {
            try
            {
                using (var context = new HrmanagementSystemContext())
                {
                    context.Set<T>().Remove(t);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
  
    }
}
