using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class ThemeProfile : Profile
    {
        public ThemeProfile()
        {            
            var m1 = CreateMap<ThemeDto, Theme>();
                m1.ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id)).PreserveLegacyUpdateBehavior("Structure", "Id");

            var m2 = CreateMap<Theme, ThemeDto>();
                m2.ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id)).PreserveLegacyUpdateBehavior("Structure", "Id");
        }
    }
}
