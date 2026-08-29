using Microsoft.Extensions.DependencyInjection;
using Ray.Repositories.Core;
using Ray.Utils.Mail;

namespace Ray.Repositories.Implementation
{
    public static class Infraestructure
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<IUserLoginRepository,UserLoginRepository>();
            services.AddScoped<IRoleRepository,RoleRepository>();
            services.AddScoped<IAssetRepository,AssetRepository>();
            services.AddScoped<IAssetMediaRepository,AssetMediaRepository>();
            services.AddScoped<INodeRepository,NodeRepository>();
            services.AddScoped<IMenuRepository,MenuRepository>();
            services.AddScoped<IAuthorRepository,AuthorRepository>();
            services.AddScoped<IKeywordRepository,KeywordRepository>();
            services.AddScoped<IMediaRepository,MediaRepository>();
            services.AddScoped<ICategoryRepository,CategoryRepository>();
            services.AddScoped<IGalleryRepository,GalleryRepository>();
            services.AddScoped<ILayoutInstanceRepository,LayoutInstanceRepository>();
            services.AddScoped<ILayoutRepository,LayoutRepository>();
            services.AddScoped<IWidgetTypeRepository,WidgetTypeRepository>();
            services.AddScoped<IWidgetRepository,WidgetRepository>();
            services.AddScoped<IProgrammingGuideRepository,ProgrammingGuideRepository>();
            services.AddScoped<ProgrammingGuideScheduleRepository,ProgrammingGuideScheduleRepository>();
            services.AddScoped<INewsSourceRepository,NewsSourceRepository>();
            services.AddScoped<ISocialNetworkRepository,SocialNetworkRepository>();
            services.AddScoped<IChannelRepository,ChannelRepository>();
            services.AddScoped<IAccessTypeRepository,AccessTypeRepository>();
            services.AddScoped<IURLRedirectRepository,URLRedirectRepository>();
            services.AddScoped<ITemplateRepository,TemplateRepository>();
            services.AddScoped<IThemeRepository,ThemeRepository>();
            services.AddScoped<IActionRepository,ActionRespository>();
            services.AddScoped<IPrintEditionRepository,PrintEditionRepository>();
            services.AddScoped<IAssetJsonRepository,AssetJsonRepository>();
            services.AddScoped<IPrintEditionNewRepository,PrintEditionNewRepository>();
            services.AddScoped<IProgramGalleryRepository,ProgramGalleryRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();


            //services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        }
    }
}
