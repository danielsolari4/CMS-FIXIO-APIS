using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class LayoutProfile : Profile
    {
        public LayoutProfile()
        {
            CreateMap<Layout, LayoutDto>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                //.ForMember(x => x.Html, opt => opt.MapFrom(src => src.Html))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }

    internal class FullLayoutProfile : Profile
    {
        public FullLayoutProfile()
        {
            CreateMap<Layout, LayoutDto>();
        }
    }
}
