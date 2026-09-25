using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IWidgetRepository : IAsyncRepository<Widget>
    { }

    public class WidgetRepository : BaseAsyncRepository<Widget>, IWidgetRepository
    {
        public WidgetRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Widgets;
        }
    }
}
