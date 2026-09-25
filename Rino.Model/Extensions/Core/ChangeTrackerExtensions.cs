using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Rino.Common.Authentication;
using Rino.Model.NewContext.Entities;
using System;
using System.Linq;

namespace Rino.Model.Extensions.Core
{
    public static class ChangeTrackerExtensions
    {
        public static void SetAuditProperties(this ChangeTracker changeTracker, ICurrentUserService currentUserService)
        {
            try
            {
                changeTracker.DetectChanges();
                IEnumerable<EntityEntry> entities =
                    changeTracker
                        .Entries()
                        .Where(t => t.Entity is IAuditableEntity &&
                        (
                             t.State == EntityState.Deleted
                            || t.State == EntityState.Added
                            || t.State == EntityState.Modified
                        ));

                if (entities.Any())
                {
                    DateTime timestamp = DateTime.UtcNow;

                    string user = currentUserService.GetCurrentUser().UserName;

                    foreach (EntityEntry entry in entities)
                    {
                        IAuditableEntity entity = (IAuditableEntity)entry.Entity;
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entity.CreationDate = timestamp;
                                entity.CreationUser = user;
                                entity.LastModificationDate = timestamp;
                                entity.LastModificationUser = user;
                                break;
                            case EntityState.Modified:
                                entity.LastModificationDate = timestamp;
                                entity.LastModificationUser = user;
                                break;
                            //case EntityState.Deleted:
                            //    if (entity is IEntityDelete)
                            //    {
                            //        entity.ModifiedDate = timestamp;
                            //        entity.ModifiedBy = user;
                            //        entry.State = EntityState.Modified;
                            //        ((IEntityDelete)entity).IsDeleted = true;
                            //    }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var asd = "S";
            }
        }
    }
}
