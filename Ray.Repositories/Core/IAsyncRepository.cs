using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ray.Repositories.Core
{
    public interface IAsyncRepository<TEntity> where TEntity : class
    {
        Task<TEntity> Add(TEntity entity);

        Task<TEntity> GetById(int id);

        Task<IQueryable<TEntity>> Get(Expression<Func<TEntity, bool>> where);

        Task<IQueryable<TEntity>> GetAll();

        Task Delete(TEntity entity);

        Task Update(TEntity entity);

        Task<int> Count(Expression<Func<TEntity, bool>> where = null);

        IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes);
    }
}
