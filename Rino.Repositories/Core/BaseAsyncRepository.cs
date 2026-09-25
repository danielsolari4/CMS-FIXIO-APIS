using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rino.Repositories.Core
{
    public abstract class BaseAsyncRepository<TEntity> where TEntity : class
    {
        public virtual IUnitOfWork UnitOfWork { get; set; }

        public virtual DbSet<TEntity> Set { get; set; }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            try
            {
                var savedEntity = Set.Add(entity);
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
                return entity;
            }
            catch (DbUpdateException e)
            {
                //TODO: logging
                throw e;
            }
            catch (Exception e)
            {
                //TODO: logging
                throw e;
            }
        }

        public virtual async Task<TEntity> GetById(int id)
        {
            return await Set.FindAsync(id);
        }

        public virtual async Task<IQueryable<TEntity>> Get(Expression<Func<TEntity, bool>> where)
        {
            return await Task.FromResult(Set.Where(where));
        }

        public virtual async Task<IQueryable<TEntity>> GetAll()
        {
            return await Task.FromResult(Set);
        }

        public virtual async Task Delete(TEntity entity)
        {
            try
            {
                Set.Remove(entity);
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            }
            catch (DbUpdateException e)
            {
                //TODO: logging
                throw e;
            }
            catch (Exception e)
            {
                //TODO: logging
                throw e;
            }
        }

        public virtual async Task Update(TEntity entity)
        {
            try
            {
                UnitOfWork.Context.Entry(entity).State = EntityState.Modified;
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                //TODO: logging
                throw e;
            }
        }

        public virtual async Task<int> Count(Expression<Func<TEntity, bool>> where = null)
        {
            return where == null ? await Set.CountAsync() : await Set.CountAsync(where);
        }

        public virtual IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes)
        {
            //UnitOfWork.Context.Configuration.LazyLoadingEnabled = false;
            DbSet<TEntity> dbSet = UnitOfWork.Context.Set<TEntity>();

            IEnumerable<TEntity> query = null;
            foreach (var include in includes)
            {
                query = dbSet.Include(include);
            }

            return query ?? dbSet;
        }
    }
}
