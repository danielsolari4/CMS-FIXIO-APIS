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

            var m1 = CreateMap<NewsSourceDto, NewsSource>();
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
