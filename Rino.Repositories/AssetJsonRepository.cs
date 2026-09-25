using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;
namespace Rino.Repositories
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