using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IAccessTypeRepository : IAsyncRepository<AccessType>
    { }

    public class AccessTypeRepository : BaseAsyncRepository<AccessType>, IAccessTypeRepository
    {
        public AccessTypeRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.AccessTypes;
        }
    }
}
