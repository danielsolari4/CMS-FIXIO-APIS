using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class NodeDto : BaseDto
    {
        public string Description { get; set; }

        public int Order { get; set; }

        public NodeDto ParentNode { get; set; }

        public int ParentNodeId { get; set; }
        public List<NodeDto> Childs { get; set; }

        public bool IsPrint { get; set; }

        public int? NewSourceId { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsDiagrammable { get; set; }

        public virtual List<NodeContentDto> Content { get; set; }

        public List<AssetDto> Assets { get; set; }

        public List<CategoryNodeDto> CategoryNode { get; set; }

        public bool IsPublished { get; set; }

        public bool UpdateAssets { get; set; } = true;



        [StringLength(150, ErrorMessage = "Title max length is 150")]
        public string SeoTitle { get; set; }

        [StringLength(300, ErrorMessage = "Description max length is 300")]
        public string SeoDescription { get; set; }

        [StringLength(300, ErrorMessage = "Keywords max length is 300")]
        public string Keywords { get; set; }

        [StringLength(300, ErrorMessage = "Keywords max length is 300")]
        public string RelatedKeywords { get; set; }

        [StringLength(150, ErrorMessage = "OGTitle max length is 150")]
        public string OGTitle { get; set; }

        [StringLength(300, ErrorMessage = "OGDescription max length is 300")]
        public string OGDescription { get; set; }
        public string SeoImage { get; set; }


        public NodeDto()
        {
            Childs = new List<NodeDto>();
            Content = new List<NodeContentDto>();
            Assets = new List<AssetDto>();
            CategoryNode = new List<CategoryNodeDto>();
        }
    }

    public class CreateNodeDtoBindingModel : NodeDto
    {
        [Required]
        public override List<NodeContentDto> Content { get; set; }

        public CreateNodeDtoBindingModel()
        {
            Content = null;
        }
    }

    public class UpdateNodeDtoBindingModel : NodeDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        public override List<NodeContentDto> Content { get; set; }

        public UpdateNodeDtoBindingModel()
        {
            Content = null;
        }
    }

    public class DeleteNodeDtoBindingModel : NodeDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
