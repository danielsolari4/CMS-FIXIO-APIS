using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;

namespace Rino.Repositories
{
    public interface ICategoryRepository : IAsyncRepository<Category>
    { }

    public class CategoryRepository : BaseAsyncRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Categories;
        }
    }
}
