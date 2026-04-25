using DataAccess.Context;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationContext _context;
       
        public GenericRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task Add(T entity)
        {
            try
            {
                _context.Set<T>().Add(entity);
            }
            catch (Exception ex)
            {

               // ErrorLog errorLog = new ErrorLog();
               // errorLog.Error = "Message:" + ex.Message + "  InnerException: " + ex.InnerException + "  ex.StackTrace:  " + ex.StackTrace;
               //await _context.AddAsync(errorLog);
               // await _context.SaveChangesAsync();
            }
            
        }

        public async Task AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }

        public IQueryable<T> GetWithoutTracking()
        => _context.Set<T>().AsNoTracking<T>();

        public IQueryable<T> GetWithTracking()
       => _context.Set<T>().AsTracking<T>();
        public async Task<T> GetByIdWithTracking(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public async Task UpdateRange(IEnumerable<T> entities)
        {
            _context.Set<T>().UpdateRange(entities);
        }
    }
}
