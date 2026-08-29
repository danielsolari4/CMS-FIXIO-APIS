using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class CreateMediaProfile : Profile
    {
        public CreateMediaProfile()
        {
            CreateMap<Media, MediaDto>()
                .ForMember(x => x.Galleries, opt => opt.Ignore())
                .ForMember(x => x.Categories, opt => opt.Ignore())
                .ForMember(x => x.AssetMedia, opt => opt.Ignore())
                .ForMember(x => x.SizesPaths, opt => opt.Ignore())
                .ForMember(x => x.Order, opt => opt.Ignore());

            CreateMap<Media, DeleteMediaDtoBindingModel>()
                .ForMember(x => x.Galleries, opt => opt.Ignore())
                .ForMember(x => x.Categories, opt => opt.Ignore())
                .ForMember(x => x.AssetMedia, opt => opt.Ignore())
                .ForMember(x => x.Order, opt => opt.Ignore());

            var m1 = CreateMap<MediaDto, Media>();
                m1.ForMember(x => x.SourcePath, opt => opt.MapFrom(src => src.SourcePath))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(x => x.FileId, opt => opt.MapFrom(src => src.FileId))
                .ForMember(x => x.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(x => x.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(x => x.FileType, opt => opt.MapFrom(src => src.FileType))
                .ForMember(x => x.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(x => x.Caption, opt => opt.MapFrom(src => src.Caption))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Height, opt => opt.MapFrom(src => src.Height))
                .ForMember(x => x.Width, opt => opt.MapFrom(src => src.Width))
                .ForMember(x => x.Keywords, opt => opt.MapFrom(src => src.Keywords))
                .ForMember(x => x.SizesPaths, opt => opt.Ignore())
                .ForMember(x => x.AlternativeText, opt => opt.MapFrom(src => src.AlternativeText))
                .ForMember(x => x.MediaUrl, opt => opt.MapFrom(src => src.MediaUrl))
                .ForMember(x => x.Featured, opt => opt.MapFrom(src => src.Featured))
                .ForMember(x => x.ViewsCount, opt => opt.MapFrom(src => src.ViewsCount))
                .ForMember(x => x.ShareCount, opt => opt.MapFrom(src => src.ShareCount))
                .ForMember(x => x.Discriminator, opt => opt.MapFrom(src => src.Discriminator))
                .ForMember(x => x.PublicationDate, opt => opt.MapFrom(src => src.PublicationDate)).PreserveLegacyUpdateBehavior("SourcePath", "IsEnabled", "FileId", "FileName", "FileSize", "FileType", "Title", "Caption", "Description", "Height", "Width", "Keywords", "SizesPaths", "AlternativeText", "MediaUrl", "Featured", "ViewsCount", "ShareCount", "Discriminator", "PublicationDate");
        }
    }

    internal class UpdateMediaProfile : Profile
    {
        public UpdateMediaProfile()
        {
            CreateMap<Media, MediaDto>()
                .ForMember(x => x.Galleries, opt => opt.Ignore())
                .ForMember(x => x.Categories, opt => opt.Ignore())
                .ForMember(x => x.SizesPaths, opt => opt.Ignore())
                .ForMember(x => x.AssetMedia, opt => opt.Ignore())
                .ForMember(x => x.Order, opt => opt.Ignore());

            var m2 = CreateMap<MediaDto, Media>();
                m2.ForMember(x => x.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(x => x.Caption, opt => opt.MapFrom(src => src.Caption))
                .ForMember(x => x.SizesPaths, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.AlternativeText, opt => opt.MapFrom(src => src.AlternativeText))
                .ForMember(x => x.Keywords, opt => opt.MapFrom(src => src.Keywords))
                .ForMember(x => x.MediaUrl, opt => opt.MapFrom(src => src.MediaUrl))
                .ForMember(x => x.Featured, opt => opt.MapFrom(src => src.Featured))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(x => x.PublicationDate, opt => opt.MapFrom(src => src.PublicationDate)).PreserveLegacyUpdateBehavior("Title", "Caption", "SizesPaths", "Description", "AlternativeText", "Keywords", "MediaUrl", "Featured", "IsEnabled", "PublicationDate");
        }
    }
}
