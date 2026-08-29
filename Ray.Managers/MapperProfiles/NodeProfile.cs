using System.Linq;
using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class NodeProfile : Profile
    {
        public NodeProfile()
        {
            CreateMap<Node, NodeDto>()
                .ForMember(x => x.ParentNode, opt => opt.Ignore())
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.Ignore())
                .ForMember(x => x.Assets, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            CreateMap<Node, DeleteNodeDtoBindingModel>()
                .ForMember(x => x.ParentNode, opt => opt.Ignore())
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.Ignore())
                .ForMember(x => x.Assets, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            CreateMap<NodeContent, NodeContentDto>();

            CreateMap<NodeDto, Node>()
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(x => x.IsPrint, opt => opt.MapFrom(src => src.IsPrint))
                .ForMember(x => x.IsDiagrammable, opt => opt.MapFrom(src => src.IsDiagrammable))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(x => x.IsPublished, opt => opt.MapFrom(src => src.IsPublished))

                .ForMember(x => x.SeoDescription, opt => opt.MapFrom(src => src.SeoDescription))
                .ForMember(x => x.SeoTitle, opt => opt.MapFrom(src => src.SeoTitle))
                .ForMember(x => x.SeoImage, opt => opt.MapFrom(src => src.SeoImage))
                .ForMember(x => x.Keywords, opt => opt.MapFrom(src => src.Keywords))
                .ForMember(x => x.OGTitle, opt => opt.MapFrom(src => src.OGTitle))
                .ForMember(x => x.NewSourceId, opt => opt.MapFrom(src => src.NewSourceId))
                .ForMember(x => x.OGDescription, opt => opt.MapFrom(src => src.OGDescription))

                .ForAllOtherMembers(x => x.Ignore());


            CreateMap<NodeContentDto, NodeContent>()
                .ForMember(x => x.Title, opt => opt.MapFrom(src => src.Title))
                .ForAllOtherMembers(x => x.Ignore());
        }
    }

    internal class TreeNodeProfile : Profile
    {
        public TreeNodeProfile()
        {
            CreateMap<Node, NodeDto>()
                .ForMember(x => x.ParentNode, opt => opt.Ignore())
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Assets, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore())
                .ForMember(x => x.Childs, opt => opt.MapFrom(c => c.Childs.Where(o => !o.IsDeleted)));

            CreateMap<Node, DeleteNodeDtoBindingModel>()
                .ForMember(x => x.ParentNode, opt => opt.Ignore())
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Assets, opt => opt.Ignore())
                .ForMember(x => x.CategoryNode, opt => opt.Ignore());

            CreateMap<NodeContent, NodeContentDto>();
        }
    }
}
