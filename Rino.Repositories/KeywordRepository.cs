using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
