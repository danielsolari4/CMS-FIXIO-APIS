using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;
namespace Ray.Repositories
{
    public interface IAssetJsonRepository : IAsyncRepository<AssetJson>
    { }

    public class AssetJsonRepository : BaseAsyncRepository<AssetJson>, IAssetJsonRepository
    {
        public AssetJsonRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.AssetJsons;
        }
    }
}