using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
