using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rino.Managers.Core
{
    public interface IManager<TEntity> where TEntity : class
    {
        Task<ICollection<TEntity>> GetAll(int? skip = null, int? take = null);

        Task<TEntity> GetById(int id);

        Task<TEntity> Add(TEntity entity);

        Task Update(TEntity entity);

        Task Delete(TEntity entity);

        Task<int> Count();
    }
}
