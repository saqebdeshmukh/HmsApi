using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.GenericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdWithTracking(int id);
        IQueryable<T> GetWithoutTracking();
        IQueryable<T> GetWithTracking();
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        Task Add(T entity);
        Task AddRange(IEnumerable<T> entities);
        Task Remove(T entity);
        Task RemoveRange(IEnumerable<T> entities);
        Task Update(T entity);
        Task UpdateRange(IEnumerable<T> entities);
    }
}
