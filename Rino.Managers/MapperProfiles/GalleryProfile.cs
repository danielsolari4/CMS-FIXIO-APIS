using AutoMapper;
using Rino.Dtos;
using Rino.Model.NewContext.Entities;

namespace Rino.Managers.MapperProfiles
{
    internal class GalleryProfile : Profile
    {
        public GalleryProfile()
        {
            CreateMap<Gallery, GalleryDto>()
                .ForMember(x => x.ComponentInstances, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore());

            CreateMap<Gallery, DeleteGalleryDtoBindingModel>()
                .ForMember(x => x.ComponentInstances, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore());

            var m1 = CreateMap<GalleryDto, Gallery>();
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled)).PreserveLegacyUpdateBehavior("Name", "IsEnabled");
        }
    }
}
