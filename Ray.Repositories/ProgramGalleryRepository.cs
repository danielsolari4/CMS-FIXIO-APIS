using Ray.Model.NewContext.Entities;
using Ray.Repositories.Core;

namespace Ray.Repositories
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
