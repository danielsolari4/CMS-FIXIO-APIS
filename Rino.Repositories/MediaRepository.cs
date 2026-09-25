using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rino.Model.NewContext.Entities;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public interface IMediaRepository : IAsyncRepository<Media>
    {
        Task<ICollection<Media>> AddImportAsync(ICollection<Media> medias);
        Task<ICollection<Media>> UpdateImportAsync(ICollection<Media> medias);
    }

    public class MediaRepository : BaseAsyncRepository<Media>, IMediaRepository
    {
        public MediaRepository(IUnitOfWork unitOfWorkInterface)
        {
            UnitOfWork = unitOfWorkInterface;
            Set = UnitOfWork.Context.Media;
        }


        public async System.Threading.Tasks.Task<ICollection<Media>> AddImportAsync(ICollection<Media> medias)
        {
            //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var item in medias)
            {
                var savedEntity = Set.Add(item);
            }

            //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            }
            //TODO: ver este catch
            //catch (DbEntityValidationException e)
            //{
            //    throw e;
            //}
            catch (Exception)
            {
                throw;
            }

            return medias;
        }


        public async System.Threading.Tasks.Task<ICollection<Media>> UpdateImportAsync(ICollection<Media> medias)
        {
            //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var item in medias)
            {
                UnitOfWork.Context.Entry(item).State = EntityState.Modified;
                //UnitOfWork.Context.Media.Attach(item);
                //UnitOfWork.Context.Entry(item).Property(x => x.SizesPaths).IsModified = true;
            }
            await UnitOfWork.Context.SaveChangesAsync(CancellationToken.None);
            //UnitOfWork.Context.Configuration.AutoDetectChangesEnabled = true;


            return medias;
        }
    }
}
