using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;
namespace Ray.Repositories
{
    public interface IGalleryRepository : IAsyncRepository<Gallery>
    { }

    public class GalleryRepository : BaseAsyncRepository<Gallery>, IGalleryRepository
    {
        public GalleryRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Galleries;
        }
    }
}
