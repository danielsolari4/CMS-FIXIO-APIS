using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
