using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;
namespace Ray.Repositories
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

