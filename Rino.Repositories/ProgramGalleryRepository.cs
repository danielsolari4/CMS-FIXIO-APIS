using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IProgramGalleryRepository : IAsyncRepository<ProgramGallery>
    { }

    public class ProgramGalleryRepository : BaseAsyncRepository<ProgramGallery>, IProgramGalleryRepository
    {
        public ProgramGalleryRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.ProgramGalleries;
        }
    }
}
