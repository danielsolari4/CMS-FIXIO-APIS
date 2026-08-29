using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Ray.Model.NewContext.Entities;

namespace Ray.Model.NewContext
{
    public partial class ModelContext 
    {
        //public override int SaveChanges()
        //{
        //    InternalSaveChanges();
        //    return base.SaveChanges();
        //}

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        //{
        //    cancellationToken = CancellationToken.None;
        //    InternalSaveChanges();
        //    return await base.SaveChangesAsync(CancellationToken.None);
        //}

        public void InternalSaveChanges()
        {

            foreach (var entityEntry in ChangeTracker.Entries())
            {
                if (entityEntry.Entity is IAuditableEntity && (entityEntry.State == (EntityState)EntityState.Added || entityEntry.State == (EntityState)EntityState.Modified))
                {

                }
            }
            var modifiedEntries = ChangeTracker.Entries()
                    .Where(x => x.Entity is IAuditableEntity
                    && (x.State == (EntityState)EntityState.Added || x.State == (EntityState) EntityState.Modified));

            foreach (var entry in modifiedEntries)
            {
                var entity = entry.Entity as IAuditableEntity;
                if (entity != null)
                {
                    var identityName = Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null ? Thread.CurrentPrincipal.Identity.Name : string.Empty;
                    var now = DateTime.UtcNow;

                    if (entry.State == (EntityState)EntityState.Added)
                    {
                        entity.CreationUser = identityName;
                        entity.CreationDate = now;
                    }
                    else
                    {
                        base.Entry(entity).Property(x => x.CreationUser).IsModified = false;
                        base.Entry(entity).Property(x => x.CreationDate).IsModified = false;
                    }

                    if (entry.State == (EntityState)EntityState.Modified)
                    {
                        entity.LastModificationUser = identityName;
                        entity.LastModificationDate = now;
                    }
                    else
                    {
                        base.Entry(entity).Property(x => x.LastModificationUser).IsModified = false;
                        base.Entry(entity).Property(x => x.LastModificationDate).IsModified = false;
                    }
                }
            }
        }
    }
}
