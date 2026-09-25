using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IAuthorRepository : IAsyncRepository<Author>
    { }

    public class AuthorRepository : BaseAsyncRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Authors;
        }
    }
}
