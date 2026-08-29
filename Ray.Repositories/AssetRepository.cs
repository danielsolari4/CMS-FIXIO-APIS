using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ray.Repositories.Core;
using Ray.Model.NewContext.Entities;
namespace Ray.Repositories
{
    public interface IAssetRepository : IAsyncRepository<Asset>
    {
        //System.Threading.Tasks.Task<ICollection<News>> AddImportAsync(ICollection<News> news);
        //System.Threading.Tasks.Task<ICollection<News>> UpdateImportAsync(ICollection<News> news);
    }

    public class AssetRepository : BaseAsyncRepository<Asset>, IAssetRepository
    {
        public AssetRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Assets;
        }

        //public async System.Threading.Tasks.Task<ICollection<News>> AddImportAsync(ICollection<News> news)
        //{
        //    //await UnitOfWork.Context.SaveChangesAsync();
        //    //UnitOfWork.Context = new ModelContext();
        //    //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = false;

        //    foreach (var item in news)
        //    {
        //        var savedEntity = Set.Add(item);
        //    }
        //    //var savedEntity = Set.AddRange(news);
        //    UnitOfWork.Context.ChangeTracker.DetectChanges();
        //    try
        //    {
        //        var res = await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
        //    }
        //    //TODO: ver este  catch
        //    //catch (DbEntityValidationException e)
        //    //{
        //    //    throw e;
        //    //}
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    return news;
        //}

        //public async System.Threading.Tasks.Task<ICollection<News>> UpdateImportAsync(ICollection<News> news)
        //{
        //    //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = false;

        //    foreach (var item in news)
        //    {
        //        UnitOfWork.Context.Entry(item).State = EntityState.Modified;
        //    }

        //    //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = true;
        //    try
        //    {
        //        await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
        //    }
        //    //TODO: ver este catch
        //    //catch (DbEntityValidationException e)
        //    //{
        //    //    throw e;
        //    //}
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (UnitOfWork.Context != null)
        //            UnitOfWork.Dispose();
        //    }

        //    return news;
        //}
    }
}
