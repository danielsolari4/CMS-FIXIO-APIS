using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class KeywordProfile : Profile
    {
        public KeywordProfile()
        {
            CreateMap<Keyword, KeywordDto>();

            CreateMap<Keyword, DeleteKeywordDtoBindingModel>();

            CreateMap<KeywordDto, Keyword>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
