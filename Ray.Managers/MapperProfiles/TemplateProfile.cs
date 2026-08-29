using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class TemplateProfile : Profile
    {
        public TemplateProfile()
        {
            CreateMap<Template, TemplateDto>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Class, opt => opt.MapFrom(src => src.Class))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }

    internal class FullTemplateProfile : Profile
    {
        public FullTemplateProfile()
        {
            CreateMap<Template, TemplateDto>();
        }
    }
}
