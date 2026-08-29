using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
