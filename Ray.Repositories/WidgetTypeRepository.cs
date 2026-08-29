using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IWidgetTypeRepository : IAsyncRepository<WidgetType>
    { }

    public class WidgetTypeRepository : BaseAsyncRepository<WidgetType>, IWidgetTypeRepository
    {
        public WidgetTypeRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.WidgetTypes;
        }
    }
}
