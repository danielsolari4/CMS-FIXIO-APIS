using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
{
    public interface IProgrammingGuideRepository : IAsyncRepository<ProgrammingGuide>
    { }

    public class ProgrammingGuideRepository : BaseAsyncRepository<ProgrammingGuide>, IProgrammingGuideRepository
    {
        public ProgrammingGuideRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.ProgrammingGuides;
        }
    }
}
