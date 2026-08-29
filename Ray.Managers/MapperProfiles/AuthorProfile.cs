using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(x => x.News, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore());

            var m1 = CreateMap<AuthorDto, Author>();
                m1.ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(x => x.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted)).PreserveLegacyUpdateBehavior("Email", "FirstName", "LastName", "IsEnabled", "IsDeleted");
        }
    }
}
