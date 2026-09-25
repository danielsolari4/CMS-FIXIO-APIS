using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;
namespace Rino.Repositories
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
