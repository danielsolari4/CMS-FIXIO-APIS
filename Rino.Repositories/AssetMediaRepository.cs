using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IAssetMediaRepository : IAsyncRepository<AssetMedia>
    {
        Task<List<int>> GetUsedMediaIdsAsync(List<int> mediaIds);
    }

    public class AssetMediaRepository : BaseAsyncRepository<AssetMedia>, IAssetMediaRepository
    {
        public AssetMediaRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.AssetMedia;
        }

        public async Task<List<int>> GetUsedMediaIdsAsync(List<int> mediaIds)
        {
            return await Set
                .Where(am => mediaIds.Contains(am.MediaId))
                .Select(am => am.MediaId)
                .Distinct()
                .ToListAsync();
        }
    }
}
