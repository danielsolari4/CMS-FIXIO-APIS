using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class NewsSourceProfile : Profile
    {
        public NewsSourceProfile()
        {
            CreateMap<NewsSource, NewsSourceDto>();

            CreateMap<NewsSourceDto, NewsSource>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}
