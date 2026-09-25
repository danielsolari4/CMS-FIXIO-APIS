using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface INewsSourceRepository : IAsyncRepository<NewsSource>
    { }

    public class NewsSourceRepository : BaseAsyncRepository<NewsSource>, INewsSourceRepository
    {
        public NewsSourceRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.NewsSources;
        }
    }
}
