using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface ILayoutInstanceRepository : IAsyncRepository<LayoutInstance>
    { }

    public class LayoutInstanceRepository : BaseAsyncRepository<LayoutInstance>, ILayoutInstanceRepository
    {
        public LayoutInstanceRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.LayoutInstances;
        }
    }
}
