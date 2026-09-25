using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
