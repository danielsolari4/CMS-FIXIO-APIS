using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;
namespace Ray.Repositories
{
    public interface IChannelRepository : IAsyncRepository<Channel>
    { }

    public class ChannelRepository : BaseAsyncRepository<Channel>, IChannelRepository
    {
        public ChannelRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Channels;
        }
    }
}
