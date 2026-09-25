using System;
using System.Linq;
using System.Linq.Expressions;

namespace Rino.Repositories.Core
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Add(TEntity entity);

        TEntity GetById(int id);

        TEntity Get(Expression<Func<TEntity, bool>> where);

        IQueryable<TEntity> GetAll();

        void Delete(TEntity entity);

        void Update(TEntity entity);

        int Count(Expression<Func<TEntity, bool>> where = null);
    }
}
