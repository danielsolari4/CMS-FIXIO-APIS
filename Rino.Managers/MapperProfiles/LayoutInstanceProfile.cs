using AutoMapper;
using Rino.Dtos;
using Rino.Model.NewContext.Entities;

namespace Rino.Managers.MapperProfiles
{
    internal class LayoutInstanceProfile : Profile
    {
        public LayoutInstanceProfile()
        {
            CreateMap<LayoutInstance, LayoutInstanceDto>();
        }
    }
}
