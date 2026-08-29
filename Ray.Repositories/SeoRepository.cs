using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ray.Dtos;
using Ray.Model.NewContext;
using Ray.Model.NewContext.Entities;

namespace Ray.Repositories
{
    public class SeoRepository
    {
        private ModelContext context;

        public SeoRepository()
        {
            context = new ModelContext();
        }

        public bool Exists(string url)
        {
            return context.SeoRedirects.Any(x => x.SeoUrl.ToLower() == url.ToLower());
        }

        public async System.Threading.Tasks.Task<bool> SaveAsync(ICollection<SeoRedirectDto> list)
        {

            var listEntities = list.Select(x => new SeoRedirect
            {
                NewUrl = x.NewUrl,
                SeoUrl = x.SeoUrl
            }).ToList();

            //context.Configuration.AutoDetectChangesEnabled = false;
            context.SeoRedirects.AddRange(listEntities);
            context.ChangeTracker.DetectChanges();
            await context.SaveChangesAsync(CancellationToken.None);
            return true;
        }
    }
}
