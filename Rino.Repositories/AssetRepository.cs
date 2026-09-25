using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Rino.Repositories.Core;
using Rino.Model.NewContext.Entities;
namespace Rino.Repositories
{
    public interface IAssetRepository : IAsyncRepository<Asset>
    {
        System.Threading.Tasks.Task<ICollection<News>> AddImportAsync(ICollection<News> news);
        System.Threading.Tasks.Task<ICollection<Asset>> UpdateImportAsync(ICollection<Asset> news);
    }

    public class AssetRepository : BaseAsyncRepository<Asset>, IAssetRepository
    {
        public AssetRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Assets;
        }

        public async System.Threading.Tasks.Task<ICollection<News>> AddImportAsync(ICollection<News> news)
        {
            UnitOfWork.Context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var item in news)
            {
                Set.Add(item);
            }

            UnitOfWork.Context.ChangeTracker.DetectChanges();

            try
            {
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                UnitOfWork.Context.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return news;
        }

        public async System.Threading.Tasks.Task<ICollection<Asset>> UpdateImportAsync(ICollection<Asset> news)
        {
            UnitOfWork.Context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var item in news)
            {
                UnitOfWork.Context.Entry(item).State = EntityState.Modified;
            }

            UnitOfWork.Context.ChangeTracker.AutoDetectChangesEnabled = true;

            try
            {
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (UnitOfWork.Context != null)
                    UnitOfWork.Dispose();
            }

            return news;
        }
    }
}
