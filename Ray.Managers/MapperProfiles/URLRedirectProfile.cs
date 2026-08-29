using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class URLRedirectProfile : Profile
    {
        public URLRedirectProfile()
        {            
            CreateMap<URLRedirectDto, URLRedirect>()
                .ForMember(x => x.From, opt => opt.MapFrom(src => src.From))
                .ForMember(x => x.To, opt => opt.MapFrom(src => src.To))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());

            CreateMap<URLRedirect, URLRedirectDto>()
                .ForMember(x => x.From, opt => opt.MapFrom(src => src.From))
                .ForMember(x => x.To, opt => opt.MapFrom(src => src.To))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
