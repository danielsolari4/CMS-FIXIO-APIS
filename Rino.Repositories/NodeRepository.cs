using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
