using AutoMapper;
using Rino.Dtos;
using Rino.Model.NewContext.Entities;

namespace Rino.Managers.MapperProfiles
{
    internal class NewsSourceProfile : Profile
    {
        public NewsSourceProfile()
        {
            CreateMap<NewsSource, NewsSourceDto>();

            var m1 = CreateMap<NewsSourceDto, NewsSource>();
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name)).PreserveLegacyUpdateBehavior("Name");
        }
    }
}
