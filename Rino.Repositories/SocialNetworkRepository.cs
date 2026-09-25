using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface ISocialNetworkRepository : IAsyncRepository<SocialNetwork>
    { }

    public class SocialNetworkRepository : BaseAsyncRepository<SocialNetwork>, ISocialNetworkRepository
    {
        public SocialNetworkRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.SocialNetworks;
        }
    }
}
