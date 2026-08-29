using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
