using AutoMapper;
using Rino.Dtos;
using Rino.Model.NewContext.Entities;

namespace Rino.Managers.MapperProfiles
{
    internal class TemplateProfile : Profile
    {
        public TemplateProfile()
        {
            var m1 = CreateMap<Template, TemplateDto>();
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Class, opt => opt.MapFrom(src => src.Class))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id)).PreserveLegacyUpdateBehavior("Name", "Class", "Id");
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
