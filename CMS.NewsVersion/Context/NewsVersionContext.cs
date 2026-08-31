using CMS.NewsVersion.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CMS.NewsVersion.Context
{
    public class NewsVersionContext : DbContext
    {
        public NewsVersionContext(DbContextOptions<NewsVersionContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=.;Database=CMSNewsVersion;Persist Security Info=True;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            }
        }


        //entities
        public DbSet<News> News { get; set; }
    }
}
