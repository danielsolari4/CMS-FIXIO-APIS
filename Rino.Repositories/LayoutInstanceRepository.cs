using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
