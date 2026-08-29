using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface INodeRepository : IAsyncRepository<Node>
    { }

    public class NodeRepository : BaseAsyncRepository<Node>, INodeRepository
    {
        public NodeRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Nodes;
        }
    }
}
