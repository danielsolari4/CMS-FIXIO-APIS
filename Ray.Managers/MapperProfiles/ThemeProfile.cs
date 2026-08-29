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
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));

            var m2 = CreateMap<Theme, ThemeDto>();
                m2.ForAllMembers(x => x.Ignore());
                m2.ForMember(x => x.Structure, opt => opt.MapFrom(src => src.Structure))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}
