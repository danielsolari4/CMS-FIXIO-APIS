using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
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
