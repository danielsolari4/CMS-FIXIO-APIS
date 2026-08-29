using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
