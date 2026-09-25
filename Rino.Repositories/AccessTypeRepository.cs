using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
