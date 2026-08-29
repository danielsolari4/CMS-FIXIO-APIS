using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class URLRedirectProfile : Profile
    {
        public URLRedirectProfile()
        {            
            var m1 = CreateMap<URLRedirectDto, URLRedirect>();
                m1.ForMember(x => x.From, opt => opt.MapFrom(src => src.From))
                .ForMember(x => x.To, opt => opt.MapFrom(src => src.To))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id)).PreserveLegacyUpdateBehavior("From", "To", "Id");

            var m2 = CreateMap<URLRedirect, URLRedirectDto>();
                m2.ForMember(x => x.From, opt => opt.MapFrom(src => src.From))
                .ForMember(x => x.To, opt => opt.MapFrom(src => src.To))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id)).PreserveLegacyUpdateBehavior("From", "To", "Id");
        }
    }
}
