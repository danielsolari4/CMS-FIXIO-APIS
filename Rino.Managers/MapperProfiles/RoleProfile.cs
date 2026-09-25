using AutoMapper;
using Rino.Dtos;
using Rino.Model.NewContext.Entities;

namespace Rino.Managers.MapperProfiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            var m1 = CreateMap<RoleDto, Role>();
                m1.ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.RoleActions, opt => opt.MapFrom(src => src.Actions)).PreserveLegacyUpdateBehavior("Id", "Name", "RoleActions");
        }
    }
}
