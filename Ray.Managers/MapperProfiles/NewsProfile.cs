using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<News, NewsDto>()
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Authors, opt => opt.Ignore())
                .ForMember(x => x.Nodes, opt => opt.Ignore())
                .ForMember(x => x.AssetMedia, opt => opt.Ignore())
                .ForMember(x => x.PrintEditionNew, opt => opt.Ignore())
                .ForMember(x => x.AssetJson, opt => opt.Ignore())
                .ForMember(x => x.Galleries, opt => opt.Ignore());

            CreateMap<PageContent, PageContentDto>()
                .ForMember(x => x.SocialNetwork, opt => opt.Ignore());

            CreateMap<NewsDto, News>()
                .ForMember(x => x.ViewsCount, opt => opt.MapFrom(src => src.ViewsCount))
                .ForMember(x => x.WasPublished, opt => opt.MapFrom(src => src.WasPublished))
                .ForMember(x => x.PublicationUser, opt => opt.MapFrom(src => src.PublicationUser))
                .ForMember(x => x.PublicationDate, opt => opt.MapFrom(src => src.PublicationDate))
                .ForMember(x => x.VotesCount, opt => opt.MapFrom(src => src.VotesCount))
                .ForMember(x => x.CommentsCount, opt => opt.MapFrom(src => src.CommentsCount))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(x => x.ShortUrl, opt => opt.MapFrom(src => src.ShortUrl))
                .ForMember(x => x.DataExtension, opt => opt.MapFrom(src => src.DataExtension))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(x => x.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
                .ForMember(x => x.EditorComments, opt => opt.MapFrom(src => src.EditorComments))
                .ForMember(x => x.NewsSourceNewsId, opt => opt.MapFrom(src => src.NewsSourceNewsId))
                .ForMember(x => x.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<PageContentDto, PageContent>()
                .ForMember(x => x.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(x => x.SocialNetworkTitle, opt => opt.MapFrom(src => src.SocialNetworkTitle))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
                .ForMember(x => x.TemplateId, opt => opt.MapFrom(src => src.TemplateId))
                .ForMember(x => x.Keywords, opt => opt.MapFrom(src => src.Keywords))
                .ForMember(x => x.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(x => x.MetaAuthor, opt => opt.MapFrom(src => src.MetaAuthor))
                .ForMember(x => x.MetaDescription, opt => opt.MapFrom(src => src.MetaDescription))
                .ForMember(x => x.MetaKeywords, opt => opt.MapFrom(src => src.MetaKeywords))
                .ForMember(x => x.Introduction, opt => opt.MapFrom(src => src.Introduction))
                .ForMember(x => x.Volanta, opt => opt.MapFrom(src => src.Volanta))
                .ForMember(x => x.MobileTitle, opt => opt.MapFrom(src => src.MobileTitle))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
