using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IKeywordRepository : IAsyncRepository<Keyword>
    { }

    public class KeywordRepository : BaseAsyncRepository<Keyword>, IKeywordRepository
    {
        public KeywordRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Keywords;
        }
    }
}
