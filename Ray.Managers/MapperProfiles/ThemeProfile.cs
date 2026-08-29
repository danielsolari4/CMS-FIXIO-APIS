using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class ThemeProfile : Profile
    {
        public ThemeProfile()
        {            
            CreateMap<ThemeDto, Theme>()
                .ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<Theme, ThemeDto>()
                .ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
