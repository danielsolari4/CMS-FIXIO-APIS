using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class LayoutProfile : Profile
    {
        public LayoutProfile()
        {
            var m1 = CreateMap<Layout, LayoutDto>();
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                //.ForMember(x => x.Html, opt => opt.MapFrom(src => src.Html))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));
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
