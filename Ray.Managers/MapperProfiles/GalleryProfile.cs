using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
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
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled));
        }
    }
}
