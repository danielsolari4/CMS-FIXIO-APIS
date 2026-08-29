using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
