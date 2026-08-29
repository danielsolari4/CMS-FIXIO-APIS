using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class LayoutInstanceProfile : Profile
    {
        public LayoutInstanceProfile()
        {
            CreateMap<LayoutInstance, LayoutInstanceDto>();
        }
    }
}
