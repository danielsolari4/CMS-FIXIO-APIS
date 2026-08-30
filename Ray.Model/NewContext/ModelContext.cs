using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Ray.Common.Authentication;
using Ray.Model.Extensions.Core;
using Ray.Model.NewContext.Entities;
using Action = Ray.Model.NewContext.Entities.Action;
using TimeZone = Ray.Model.NewContext.Entities.TimeZone;

#nullable disable

namespace Ray.Model.NewContext
{
    public partial class ModelContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ModelContext()
        {
        }

        private readonly ICurrentUserService _currentUserService;
        public ModelContext(DbContextOptions<ModelContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }
        #region DbSets

        public virtual DbSet<AccessType> AccessTypes { get; set; }
        public virtual DbSet<Ray.Model.NewContext.Entities.Action> Actions { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetAsset> AssetAssets { get; set; }
        public virtual DbSet<AssetContent> AssetContents { get; set; }
        public virtual DbSet<AssetGallery> AssetGalleries { get; set; }
        public virtual DbSet<AssetJson> AssetJsons { get; set; }
        public virtual DbSet<AssetMedia> AssetMedia { get; set; }
        public virtual DbSet<AssetNode> AssetNodes { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryNode> CategoryNodes { get; set; }
        public virtual DbSet<Channel> Channels { get; set; }
        public virtual DbSet<Gallery> Galleries { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupAction> GroupActions { get; set; }
        public virtual DbSet<Keyword> Keywords { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Layout> Layouts { get; set; }
        public virtual DbSet<LayoutInstance> LayoutInstances { get; set; }
        public virtual DbSet<MediaCategory> MediaCategories { get; set; }
        public virtual DbSet<MediaGallery> MediaGalleries { get; set; }
        public virtual DbSet<MediaKeyword> MediaKeywords { get; set; }
        public virtual DbSet<Media> Media { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<NewsAuthor> NewsAuthors { get; set; }
        public virtual DbSet<NewsSource> NewsSources { get; set; }
        public virtual DbSet<Node> Nodes { get; set; }
        public virtual DbSet<NodeContent> NodeContents { get; set; }
        public virtual DbSet<NodeKeyword> NodeKeywords { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<PageContent> PageContents { get; set; }
        public virtual DbSet<PageContentKeyword> PageContentKeywords { get; set; }
        public virtual DbSet<PageContentSocialNetwork> PageContentSocialNetworks { get; set; }
        public virtual DbSet<PrintEdition> PrintEditions { get; set; }
        public virtual DbSet<PrintEditionNew> PrintEditionNews { get; set; }
        public virtual DbSet<ProgramGallery> ProgramGalleries { get; set; }
        public virtual DbSet<ProgrammingGuide> ProgrammingGuides { get; set; }
        public virtual DbSet<ProgrammingGuideMedia> ProgrammingGuideMedia { get; set; }
        public virtual DbSet<ProgrammingGuideNode> ProgrammingGuideNodes { get; set; }
        public virtual DbSet<ProgrammingGuideSchedule> ProgrammingGuideSchedules { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleAction> RoleActions { get; set; }
        public virtual DbSet<SeoRedirect> SeoRedirects { get; set; }
        public virtual DbSet<SocialNetwork> SocialNetworks { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<Ray.Model.NewContext.Entities.TimeZone> TimeZones { get; set; }
        public virtual DbSet<URLRedirect> URLRedirects { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAsset> UserAssets { get; set; }
        public virtual DbSet<UserClaim> UserClaims { get; set; }
        public virtual DbSet<UserLogin> UserLogins { get; set; }
        public virtual DbSet<UserNode> UserNodes { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Widget> Widgets { get; set; }
        public virtual DbSet<WidgetType> WidgetTypes { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
        public virtual DbSet<WorkflowSetting> WorkflowSettings { get; set; }
        public virtual DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public virtual DbSet<WorkflowStepAction> WorkflowStepActions { get; set; }
        public virtual DbSet<WorkflowStepSetting> WorkflowStepSettings { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<GetTreeNodeStructure_Result> GetTreeNodeStructure_Result { get; set; }
        public virtual DbSet<GetTreeCategoryStructure_Result> GetTreeCategoryStructure_Result { get; set; }
        public virtual DbSet<GetTreeMicrosite_Result> GetTreeMicrosite_Result { get; set; }
        public virtual DbSet<GetTreeLayoutStructureById_Result> GetTreeLayoutStructureById_Result { get; set; }
        #endregion



        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.SetAuditProperties(_currentUserService);
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ChangeTracker.SetAuditProperties(_currentUserService);
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ChangeTracker.SetAuditProperties(_currentUserService);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ChangeTracker.SetAuditProperties(_currentUserService);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
                if (string.IsNullOrWhiteSpace(cs))
                {
                    cs = "Server=.;Database=fixioCMS;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";
                }
                optionsBuilder.UseSqlServer(NormalizeSqlServerConnectionString(cs));
            }
            //http://localhost:8983/

            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        private static string NormalizeSqlServerConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            var normalized = connectionString.Trim();
            if (!normalized.EndsWith(";"))
            {
                normalized += ";";
            }

            if (!normalized.Contains("Encrypt=", StringComparison.OrdinalIgnoreCase))
            {
                normalized += "Encrypt=False;";
            }

            if (!normalized.Contains("TrustServerCertificate=", StringComparison.OrdinalIgnoreCase))
            {
                normalized += "TrustServerCertificate=True;";
            }

            return normalized;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<IdentityRoleClaim<int>>();
            modelBuilder.Ignore<IdentityUserToken<int>>();

            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AccessType>(entity =>
            {
                entity.ToTable("AccessType");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Action>(entity =>
            {
                entity.ToTable("Action");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => new { e.ControllerName, e.ActionName }, "ControllerNameActionNameIndex")
                    .IsUnique();

                entity.Property(e => e.ActionName).HasMaxLength(100);

                entity.Property(e => e.Code).HasMaxLength(5);

                entity.Property(e => e.ControllerName).HasMaxLength(100);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.GroupAction)
                    .WithMany(p => p.Actions)
                    .HasForeignKey(d => d.GroupActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Action_GroupAction");
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Asset");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Asset");

                entity.HasIndex(e => e.DataExtension, "NonClusteredIndex-20190510-174019");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.DataExtension).HasMaxLength(50);

                entity.Property(e => e.Discriminator)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.AccessType)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.AccessTypeId)
                    .HasConstraintName("FK_Asset_AccessType");


            });

            modelBuilder.Entity<GetTreeNodeStructure_Result>().HasNoKey().ToView(null);
            modelBuilder.Entity<GetTreeCategoryStructure_Result>().HasNoKey().ToView(null);
            modelBuilder.Entity<GetTreeMicrosite_Result>().HasNoKey().ToView(null);
            modelBuilder.Entity<GetTreeLayoutStructureById_Result>().HasNoKey().ToView(null);

            modelBuilder.Entity<AssetAsset>(entity =>
            {
                entity.HasKey(e => new { e.AssetId, e.RelatedAssetId });

                entity.ToTable("AssetAsset");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AssetAssetAssets)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetAsset_Asset");

                entity.HasOne(d => d.RelatedAsset)
                    .WithMany(p => p.AssetAssetRelatedAssets)
                    .HasForeignKey(d => d.RelatedAssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetAsset_RelatedAsset");
            });

            modelBuilder.Entity<AssetContent>(entity =>
            {
                entity.ToTable("AssetContent");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => new { e.AssetId, e.LanguageId }, "AssetLanguageIndex")
                    .IsUnique();

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.Discriminator)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Introduction).HasMaxLength(4000);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.MobileTitle)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.SocialNetworkTitle).HasMaxLength(1000);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Volanta).HasMaxLength(1000);

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AssetContents)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetContent_Asset");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.AssetContents)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetContent_Language");

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.AssetContents)
                    .HasForeignKey(d => d.TemplateId)
                    .HasConstraintName("FK_AssetContent_Template");
            });

            modelBuilder.Entity<AssetGallery>(entity =>
            {
                entity.HasKey(e => new { e.AssetId, e.GalleryId })
                    .HasName("PK_dbo.AssetGallery");

                entity.ToTable("AssetGallery");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.Galleries)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetGallery_Asset");

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.AssetGalleries)
                    .HasForeignKey(d => d.GalleryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetGallery_Gallery");
            });

            modelBuilder.Entity<AssetJson>(entity =>
            {
                entity.ToTable("AssetJson");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AssetJsons)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetJson_Asset");
            });

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.ToTable("Settings");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AssetMedia>(entity =>
            {
                entity.HasKey(e => new { e.AssetId, e.MediaId })
                    .HasName("PK_dbo.AssetMedia");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AssetMedia)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetMedia_Asset");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.AssetMedia)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetMedia_Media");
            });

            modelBuilder.Entity<AssetNode>(entity =>
            {
                entity.HasKey(e => new { e.AssetId, e.NodeId });

                entity.ToTable("AssetNode");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.AssetNodes)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetNode_Asset");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.AssetNodes)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssetNode_Node");
            });

            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Author");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Author");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Authors)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_Author_MediaId");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Category");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Childs)
                    .HasForeignKey(d => d.ParentCategoryId)
                    .HasConstraintName("FK_dbo.Category_dbo.Category_ParentCategoryId");
            });

            modelBuilder.Entity<CategoryNode>(entity =>
            {
                entity.HasKey(e => new { e.CategoryId, e.NodeId });

                entity.ToTable("CategoryNode");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CategoryNodes)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryNode_Category");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.CategoryNodes)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CategoryNode_Node");
            });

            modelBuilder.Entity<Channel>(entity =>
            {
                entity.ToTable("Channel");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Channels)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("FK_Channel_Media");
            });

            modelBuilder.Entity<Gallery>(entity =>
            {
                entity.ToTable("Gallery");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Gallery");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("Group");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<GroupAction>(entity =>
            {
                entity.ToTable("GroupAction");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupActions)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("FK_GroupAction_Group");
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.ToTable("Keyword");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Keyword");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(256);
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("Language");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CultureName)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Layout>(entity =>
            {
                entity.ToTable("Layout");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Structure)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<LayoutInstance>(entity =>
            {
                entity.ToTable("LayoutInstance");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.LayoutType).HasDefaultValueSql("((1))");

                entity.Property(e => e.PublicationDate).HasColumnType("datetime");

                entity.Property(e => e.Structure).IsRequired();

                entity.HasOne(d => d.Layout)
                    .WithMany(p => p.LayoutInstances)
                    .HasForeignKey(d => d.LayoutId)
                    .HasConstraintName("FK_LayoutInstance_Layout");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.LayoutInstances)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LayoutInstance_Node");

                entity.HasOne(d => d.PrintEdition)
                    .WithMany(p => p.LayoutInstances)
                    .HasForeignKey(d => d.PrintEditionId)
                    .HasConstraintName("FK_LayoutInstance_PrintEdition");
            });

            modelBuilder.Entity<MediaCategory>(entity =>
            {
                entity.HasKey(e => new { e.MediaId, e.CategoryId })
                    .HasName("PK_dbo.MediaCategory");

                entity.ToTable("MediaCategory");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.MediaCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaCategory_Category");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaCategories)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaCategory_Media");
            });

            modelBuilder.Entity<MediaGallery>(entity =>
            {
                entity.HasKey(e => new { e.MediaId, e.GalleryId })
                    .HasName("PK_dbo.MediaGallery");

                entity.ToTable("MediaGallery");

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.MediaGalleries)
                    .HasForeignKey(d => d.GalleryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaGallery_Gallery");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaGalleries)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaGallery_Media");
            });

            modelBuilder.Entity<MediaKeyword>(entity =>
            {
                entity.HasKey(e => new { e.MediaId, e.KeywordId })
                    .HasName("PK_dbo.MediaKeyword");

                entity.ToTable("MediaKeyword");

                entity.HasOne(d => d.Keyword)
                    .WithMany(p => p.MediaKeywords)
                    .HasForeignKey(d => d.KeywordId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaKeyword_Keyword");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.MediaKeywords)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MediaKeyword_Media");
            });

            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Media");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Caption).HasMaxLength(256);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.Discriminator)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.FileId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FileType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.PublicationDate).HasColumnType("datetime");

                entity.Property(e => e.SizesPaths).IsRequired();

                entity.Property(e => e.SourcePath)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Structure)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.ToTable("News");
                //entity.Property(et => et.Id).ValueGeneratedNever();

                entity.Property(e => e.ExpirationDate).HasColumnType("datetime");

                entity.Property(e => e.NewsSourceNewsId).HasMaxLength(256);

                entity.HasOne(d => d.NewsSource)
                    .WithMany(p => p.News)
                    .HasForeignKey(d => d.NewsSourceId)
                    .HasConstraintName("FK_News_NewsSource");
            });

            modelBuilder.Entity<NewsAuthor>(entity =>
            {
                entity.HasKey(e => new { e.NewsId, e.AuthorId })
                    .HasName("PK_dbo.NewsAuthor");

                entity.ToTable("NewsAuthor");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.NewsAuthors)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NewsAuthor_Author");

                entity.HasOne(d => d.News)
                    .WithMany(p => p.NewsAuthors)
                    .HasForeignKey(d => d.NewsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NewsAuthor_News");
            });

            modelBuilder.Entity<NewsSource>(entity =>
            {
                entity.ToTable("NewsSource");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Node>(entity =>
            {
                entity.ToTable("Node");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_Node");

                entity.HasIndex(e => e.ParentNodeId, "NonClusteredIndex-20190522-122553");

                entity.HasIndex(e => e.IsDeleted, "NonClusteredIndex-20190625-105638");

                entity.HasIndex(e => new { e.Id, e.ParentNodeId }, "NonClusteredIndex-20191028-123745");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.IsEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Keywords)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.OGDescription)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("OGDescription");

                entity.Property(e => e.OGTitle)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("OGTitle");

                entity.Property(e => e.SeoDescription)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.SeoImage).IsUnicode(false);

                entity.Property(e => e.SeoTitle)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.NewSource)
                    .WithMany(p => p.Nodes)
                    .HasForeignKey(d => d.NewSourceId)
                    .HasConstraintName("FK_Node_NewSource");

                entity.HasOne(d => d.ParentNode)
                    .WithMany(p => p.Childs)
                    .HasForeignKey(d => d.ParentNodeId)
                    .HasConstraintName("FK_dbo.Node_dbo.Node_ParentNodeId");
            });

            modelBuilder.Entity<NodeContent>(entity =>
            {
                entity.ToTable("NodeContent");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.NodeId, "NonClusteredIndex-20190625-110029");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.NodeContents)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NodeContent_Language");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.Content)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NodeContent_Node");
            });

            modelBuilder.Entity<NodeKeyword>(entity =>
            {
                entity.HasKey(e => new { e.NodeId, e.KeywordId });

                entity.ToTable("NodeKeyword");

                entity.HasOne(d => d.Keyword)
                    .WithMany(p => p.NodeKeywords)
                    .HasForeignKey(d => d.KeywordId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NodeKeyword_Keyword");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.NodeKeywords)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NodeKeyword_Node");
            });

            modelBuilder.Entity<Page>(entity =>
            {
                entity.ToTable("Page");

                //entity.Property(et => et.Id).ValueGeneratedNever();

                entity.Property(e => e.PublicationDate).HasColumnType("datetime");

                entity.Property(e => e.PublicationUser).HasMaxLength(256);

                entity.Property(e => e.ShortUrl).HasMaxLength(100);

                entity.Property(e => e.Url).HasMaxLength(300);
            });

            modelBuilder.Entity<PageContent>(entity =>
            {
                entity.ToTable("PageContent");
                //entity.Property(et => et.Id).ValueGeneratedNever();

                entity.Property(e => e.BackgroundColor).HasMaxLength(50);
            });

            modelBuilder.Entity<PageContentKeyword>(entity =>
            {
                entity.HasKey(e => new { e.PageContentId, e.KeywordId })
                    .HasName("PK_dbo.PageContentKeyword");

                entity.ToTable("PageContentKeyword");

                entity.HasOne(d => d.Keyword)
                    .WithMany(p => p.PageContentKeywords)
                    .HasForeignKey(d => d.KeywordId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NewsKeyword_Keyword");

                entity.HasOne(d => d.PageContent)
                    .WithMany(p => p.PageContentKeywords)
                    .HasForeignKey(d => d.PageContentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NewsKeyword_PageContent");
            });

            modelBuilder.Entity<PageContentSocialNetwork>(entity =>
            {
                entity.HasKey(e => new { e.PageContentId, e.SocialNetworkId });

                entity.ToTable("PageContentSocialNetwork");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasOne(d => d.PageContent)
                    .WithMany(p => p.PageContentSocialNetworks)
                    .HasForeignKey(d => d.PageContentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PageContentSocialNetwork_PageContent");

                entity.HasOne(d => d.SocialNetwork)
                    .WithMany(p => p.PageContentSocialNetworks)
                    .HasForeignKey(d => d.SocialNetworkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PageContentSocialNetwork_SocialNetwork");
            });

            modelBuilder.Entity<PrintEdition>(entity =>
            {
                entity.ToTable("PrintEdition");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.Edition)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Identifier)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.MigrationDate).HasColumnType("datetime");

                entity.Property(e => e.Structure).IsUnicode(false);

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.PrintEditions)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrintEdition_Node");
            });

            modelBuilder.Entity<PrintEditionNew>(entity =>
            {
                entity.ToTable("PrintEditionNew");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Page)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.New)
                    .WithMany(p => p.PrintEditionNews)
                    .HasForeignKey(d => d.NewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrintEditionNew_New");

                entity.HasOne(d => d.PrintEdition)
                    .WithMany(p => p.PrintEditionNews)
                    .HasForeignKey(d => d.PrintEditionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrintEditionNew_PrintEdition");
            });

            modelBuilder.Entity<ProgramGallery>(entity =>
            {
                entity.ToTable("ProgramGallery");

                entity.HasOne(d => d.Gallery)
                    .WithMany(p => p.ProgramGalleries)
                    .HasForeignKey(d => d.GalleryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgramGallery_Gallery1");

                entity.HasOne(d => d.Program)
                    .WithMany(p => p.ProgramGalleries)
                    .HasForeignKey(d => d.ProgramId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgramGallery_ProgrammingGuide1");
            });

            modelBuilder.Entity<ProgrammingGuide>(entity =>
            {
                entity.ToTable("ProgrammingGuide");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.CacheSolr, "IX_CacheSolr_ProgrammingGuide");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.MobileUrl).HasMaxLength(256);

                entity.Property(e => e.ProgramName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SecondaryUrl).HasMaxLength(256);

                entity.Property(e => e.Url).HasMaxLength(256);

                entity.HasOne(d => d.ChannelNavigation)
                    .WithMany(p => p.ProgrammingGuides)
                    .HasForeignKey(d => d.Channel)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuide_Channels");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.ProgrammingGuides)
                    .HasForeignKey(d => d.NodeId)
                    .HasConstraintName("FK_ProgrammingGuide_Node");
            });

            modelBuilder.Entity<ProgrammingGuideMedia>(entity =>
            {
                entity.HasKey(e => new { e.ProgrammingGuideId, e.MediaId });

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ProgrammingGuideMedia)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuideMedia_Media");

                entity.HasOne(d => d.ProgrammingGuide)
                    .WithMany(p => p.ProgrammingGuideMedia)
                    .HasForeignKey(d => d.ProgrammingGuideId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuideMedia_ProgrammingGuide");
            });

            modelBuilder.Entity<ProgrammingGuideNode>(entity =>
            {
                entity.HasKey(e => new { e.ProgrammingGuideId, e.NodeId });

                entity.ToTable("ProgrammingGuideNode");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.ProgrammingGuideNodes)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuideNode_Node");

                entity.HasOne(d => d.ProgrammingGuide)
                    .WithMany(p => p.ProgrammingGuideNodes)
                    .HasForeignKey(d => d.ProgrammingGuideId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuideNode_ProgrammingGuide");
            });

            modelBuilder.Entity<ProgrammingGuideSchedule>(entity =>
            {
                entity.ToTable("ProgrammingGuideSchedule");

                entity.HasOne(d => d.ProgrammingGuide)
                    .WithMany(p => p.ProgrammingGuideSchedules)
                    .HasForeignKey(d => d.ProgrammingGuideId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProgrammingGuideSchedule_ProgrammingGuide");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Ignore(e => e.ConcurrencyStamp);
                entity.Ignore(e => e.NormalizedName);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<RoleAction>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ActionId })
                    .HasName("PK_dbo.RoleAction");

                entity.ToTable("RoleAction");

                entity.HasIndex(e => e.ActionId, "IX_ActionId");

                entity.HasIndex(e => e.RoleId, "IX_RoleId");

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.RoleActions)
                    .HasForeignKey(d => d.ActionId)
                    .HasConstraintName("FK_dbo.RoleAction_dbo.Action_ActionId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleActions)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.RoleAction_dbo.Role_RoleId");
            });

            modelBuilder.Entity<SeoRedirect>(entity =>
            {
                entity.ToTable("SeoRedirect");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.NewUrl)
                    .IsRequired()
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.SeoUrl)
                    .IsRequired()
                    .HasMaxLength(400)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SocialNetwork>(entity =>
            {
                entity.ToTable("SocialNetwork");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Template");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Class)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Theme>(entity =>
            {
                entity.ToTable("Theme");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Structure)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TimeZone>(entity =>
            {
                entity.ToTable("TimeZone");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Abbr)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsFixedLength(true);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Utcoffset).HasColumnName("UTCOffset");
            });

            modelBuilder.Entity<URLRedirect>(entity =>
            {
                entity.ToTable("URLRedirect");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.From).HasMaxLength(400);

                entity.Property(e => e.To).HasMaxLength(400);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Ignore(e => e.ConcurrencyStamp);
                entity.Ignore(e => e.LockoutEnd);
                entity.Ignore(e => e.NormalizedEmail);
                entity.Ignore(e => e.NormalizedUserName);

                entity.HasIndex(e => new { e.Id, e.Discriminator }, "IX_Discriminator_Id_User");

                entity.HasIndex(e => e.FacebookId, "IX_FacebookId_User");

                entity.HasIndex(e => e.TwitterId, "IX_TwitterId_User");

                entity.HasIndex(e => e.UserName, "UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Address).HasMaxLength(256);

                entity.Property(e => e.BirthDate).HasColumnType("smalldatetime");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CellPhone).HasMaxLength(100);

                entity.Property(e => e.City).HasMaxLength(100);

                entity.Property(e => e.Country).HasMaxLength(100);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.Discriminator)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FacebookId).HasMaxLength(50);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.IdentificationNumber).HasMaxLength(50);

                entity.Property(e => e.Imei)
                    .HasMaxLength(50)
                    .HasColumnName("IMEI");

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.Neighborhood).HasMaxLength(300);

                entity.Property(e => e.ProfileImagePath).HasMaxLength(300);

                entity.Property(e => e.State).HasMaxLength(100);

                entity.Property(e => e.TwitterId).HasMaxLength(50);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ZipCode).HasMaxLength(50);

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Language");

                entity.HasOne(d => d.TimeZone)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.TimeZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_TimeZone");
            });

            modelBuilder.Entity<UserAsset>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.AssetId });

                entity.ToTable("UserAsset");

                entity.HasOne(d => d.Asset)
                    .WithMany(p => p.UserAssets)
                    .HasForeignKey(d => d.AssetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAsset_Asset");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserAssets)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAsset_User");
            });

            modelBuilder.Entity<UserClaim>(entity =>
            {
                entity.ToTable("UserClaim");

                entity.HasIndex(e => e.UserId, "IX_UserId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserClaim_dbo.User_UserId");
            });

            modelBuilder.Entity<UserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("PK_dbo.UserLogin");

                entity.ToTable("UserLogin");

                entity.HasIndex(e => e.UserId, "IX_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Ignore(e => e.ProviderDisplayName);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserLogin_dbo.User_UserId");
            });

            modelBuilder.Entity<UserNode>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.NodeId });

                entity.ToTable("UserNode");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UserNodes)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserNode_Node");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserNodes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserNode_User");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_dbo.UserRole");

                entity.ToTable("UserRole");

                entity.HasIndex(e => e.RoleId, "IX_RoleId");

                entity.HasIndex(e => e.UserId, "IX_UserId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.UserRole_dbo.Role_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserRole_dbo.User_UserId");
            });

            modelBuilder.Entity<Widget>(entity =>
            {
                entity.ToTable("Widget");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.HasIndex(e => e.WidgetTypeId, "IDX_WidgetTypeId");

                entity.Property(e => e.CacheSolr)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Html)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Icon)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.WidgetType)
                    .WithMany(p => p.Widgets)
                    .HasForeignKey(d => d.WidgetTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Widget_WidgetType");
            });

            modelBuilder.Entity<WidgetType>(entity =>
            {
                entity.ToTable("WidgetType");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.Icon)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.ToTable("Workflow");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<WorkflowSetting>(entity =>
            {
                entity.ToTable("WorkflowSetting");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.WorkflowSettings)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.WorkflowSetting_dbo.Workflow_WorkflowId");
            });

            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.ToTable("WorkflowStep");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.Workflow)
                    .WithMany(p => p.WorkflowSteps)
                    .HasForeignKey(d => d.WorkflowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.WorkflowStep_dbo.Workflow_WorkflowId");
            });

            modelBuilder.Entity<WorkflowStepAction>(entity =>
            {
                entity.HasKey(e => new { e.WorkflowStepId, e.ActionId })
                    .HasName("PK_dbo.WorkflowStepAction");

                entity.ToTable("WorkflowStepAction");

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.WorkflowStepActions)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.WorkflowStepAction_dbo.Action_ActionId");

                entity.HasOne(d => d.WorkflowStep)
                    .WithMany(p => p.WorkflowStepActions)
                    .HasForeignKey(d => d.WorkflowStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.WorkflowStepAction_dbo.WorkflowStep_WorkflowStepId");
            });

            modelBuilder.Entity<WorkflowStepSetting>(entity =>
            {
                entity.ToTable("WorkflowStepSetting");
                entity.Property(f => f.Id).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                entity.Property(e => e.CreationDate).HasColumnType("datetime");

                entity.Property(e => e.CreationUser).HasMaxLength(256);

                entity.Property(e => e.LastModificationDate).HasColumnType("datetime");

                entity.Property(e => e.LastModificationUser).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasOne(d => d.WorkflowStep)
                    .WithMany(p => p.WorkflowStepSettings)
                    .HasForeignKey(d => d.WorkflowStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.WorkflowStepSetting_dbo.WorkflowStep_WorkflowStepId");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public virtual List<GetTreeCategoryStructure_Result> GetTreeCategoryStructure()
        {
            try
            {
                return Set<GetTreeCategoryStructure_Result>()
                     .FromSqlRaw("dbo.GetTreeCategoryStructure")
                     .ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public virtual List<GetTreeLayoutStructureById_Result> GetTreeLayoutStructureById(Nullable<int> nodeid)
        {
            object[] sqlParams = {
                new SqlParameter("@nodeid", nodeid),
                new SqlParameter("nodeid", typeof(int))
            };

            return Set<GetTreeLayoutStructureById_Result>()
                .FromSqlRaw("dbo.GetTreeLayoutStructureById", sqlParams)
                .ToList();
        }

        public virtual List<GetTreeMicrosite_Result> GetTreeMicrosite()
        {
            try
            {
                return Set<GetTreeMicrosite_Result>()
                     .FromSqlRaw("dbo.GetTreeMicrosite")
                     .ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public virtual List<GetTreeNodeStructure_Result> GetTreeNodeStructure()
        {
            try
            {
                return Set<GetTreeNodeStructure_Result>()
                     .FromSqlRaw("dbo.GetTreeNodeStructure")
                     .ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
