using System.Linq;
using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.ParentCategory, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            CreateMap<Category, DeleteCategoryDtoBindingModel>()
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.ParentCategory, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            CreateMap<MediaCategory, DeleteCategoryDtoBindingModel>()
               .ForMember(x => x.Media, opt => opt.Ignore())
               .ForMember(x => x.ParentCategory, opt => opt.Ignore())
               .ForMember(x => x.Childs, opt => opt.Ignore())
               .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            var m1 = CreateMap<CategoryDto, Category>();
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled));
        }
    }

    internal class TreeCategoryProfile : Profile
    {
        public TreeCategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(x => x.ParentCategory, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.MapFrom(c => c.Childs.Where(o => o.IsEnabled)));

            CreateMap<Category, DeleteCategoryDtoBindingModel>()
                .ForMember(x => x.ParentCategory, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());
        }
    }
}
