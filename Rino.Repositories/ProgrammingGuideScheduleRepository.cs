using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public class ProgrammingGuideScheduleRepository : BaseAsyncRepository<ProgrammingGuideSchedule>, IAsyncRepository<ProgrammingGuideSchedule>
    {
        public ProgrammingGuideScheduleRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.ProgrammingGuideSchedules;
        }
    }
}
