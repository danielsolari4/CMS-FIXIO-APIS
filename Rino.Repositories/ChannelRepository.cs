using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;
namespace Rino.Repositories
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
