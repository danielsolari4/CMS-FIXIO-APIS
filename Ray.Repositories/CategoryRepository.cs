using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;

namespace Ray.Repositories
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
