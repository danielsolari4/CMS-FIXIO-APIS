using Microsoft.Extensions.DependencyInjection;

namespace Ray.Managers.Implementation
{
    public static class Infraestructure
    {
        public static void AddManagers(this IServiceCollection services)
        {
            services.AddScoped<ApplicationUserManager, ApplicationUserManager>();
            services.AddScoped<IApplicationUserManager, Managers.ApplicationUserManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IPageManager, PageManager>();
            services.AddScoped<INewsManager, NewsManager>();
            services.AddScoped<INodeManager, NodeManager>();
            services.AddScoped<IMenuManager, MenuManager>();
            services.AddScoped<IAuthorManager, AuthorManager>();
            services.AddScoped<IKeywordManager, KeywordManager>();
            services.AddScoped<IMediaManager, MediaManager>();
            services.AddScoped<IURLRedirectManager, URLRedirectManager>();
            services.AddScoped<ICategoryManager, CategoryManager>();
            services.AddScoped<IGalleryManager, GalleryManager>();
            services.AddScoped<ILayoutInstanceManager, LayoutInstanceManager>();
            services.AddScoped<ILayoutManager, LayoutManager>();
            services.AddScoped<ITemplateManager, TemplateManager>();
            services.AddScoped<IProgrammingGuideManager, ProgrammingGuideManager>();
            services.AddScoped<IChannelManager, ChannelManager>();
            services.AddScoped<IAccessTypeManager, AccessTypeManager>();
            services.AddScoped<IThemeManager, ThemeManager>();
            services.AddScoped<IWidgetManager, WidgetManager>();
            services.AddScoped<IWeatherManager, WeatherManager>();
            services.AddScoped<IRoleManager, RoleManager>();
            services.AddScoped<INewsSourceManager, NewsSourceManager>();
            services.AddScoped<IPrintEditionManager, PrintEditionManager>();
            services.AddScoped<IBackloadManager, BackloadManager>();
            services.AddScoped<IAmazonS3Manager, AmazonS3Manager>();
            services.AddScoped<IStatsManager, StatsManager>();
            services.AddScoped<ISettingsManager, SettingsManager>();
            services.AddScoped<IPaywallManager, PaywallManager>();
            services.AddScoped<ICacheInvalidationManager, CacheInvalidationManager>();

            //services.AddScoped<IRegionRepository, RegionRepository>();
            //services.AddScoped<IMockupRepository, MockupRepository>();
            //services.AddScoped<IProductFormatRepository, ProductFormatRepository>();
            //services.AddScoped<IProductProcessRepository, ProductProcessRepository>();
            //services.AddScoped<IProductRepository, ProductRepository>();
            //services.AddScoped<IPatternRepository, PatternRepository>();
            //services.AddScoped<IBackgroundRepository, BackgroundRepository>();
            //services.AddScoped<IElementRepository, ElementRepository>();
            //services.AddScoped<IFeaturedRepository, FeaturedRepository>();
            //services.AddScoped<IUserRepository, UserRepository>();
            //services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            //services.AddScoped<IUserRegionRepository, UserRegionRepository>();
            //services.AddScoped<IRoleRepository, RoleRepository>();
            //services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            //services.AddScoped<IClientRepository, ClientRepository>();

            //services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        }
    }
}
