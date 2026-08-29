using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IMenuRepository : IAsyncRepository<Menu>
    { }

    public class MenuRepository : BaseAsyncRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Menus;
        }
    }
}
