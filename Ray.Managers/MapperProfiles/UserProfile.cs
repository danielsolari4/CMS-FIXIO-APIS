using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.Node, opt => opt.Ignore())
                .ForMember(x => x.Assets, opt => opt.Ignore());

            var m1 = CreateMap<UserDto, User>();
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(x => x.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(x => x.State, opt => opt.MapFrom(src => src.State))
                .ForMember(x => x.City, opt => opt.MapFrom(src => src.City))
                .ForMember(x => x.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(x => x.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(x => x.CellPhone, opt => opt.MapFrom(src => src.CellPhone))
                .ForMember(x => x.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
                .ForMember(x => x.TimeZoneId, opt => opt.MapFrom(src => src.TimeZoneId))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.FacebookId, opt => opt.MapFrom(src => src.FacebookId))
                .ForMember(x => x.TwitterId, opt => opt.MapFrom(src => src.TwitterId))
                .ForMember(x => x.IdentificationNumber, opt => opt.MapFrom(src => src.IdentificationNumber))
                .ForMember(x => x.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(x => x.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(x => x.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
                .ForMember(x => x.Reference, opt => opt.MapFrom(src => src.Reference))
                .ForMember(x => x.Imei, opt => opt.MapFrom(src => src.IMEI));
        }
    }
}
