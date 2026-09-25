using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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