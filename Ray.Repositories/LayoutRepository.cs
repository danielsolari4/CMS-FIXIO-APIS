using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface ILayoutRepository : IAsyncRepository<Layout>
    { }

    public class LayoutRepository : BaseAsyncRepository<Layout>, ILayoutRepository
    {
        public LayoutRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Layouts;
        }
    }
}