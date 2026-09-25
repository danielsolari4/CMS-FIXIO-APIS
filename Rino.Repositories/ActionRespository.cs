using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;
namespace Rino.Repositories
{
    public interface IActionRepository : IAsyncRepository<Action>
    { }


    public class ActionRespository : BaseAsyncRepository<Action>, IActionRepository
    {
        public ActionRespository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Actions;
        }
    }
}

