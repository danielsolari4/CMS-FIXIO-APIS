using System;
using System.Collections.Generic;

namespace Rino.Dtos
{
    public abstract class AssetDto : BaseDto
    {
        public virtual string Status { get; set; }
        public virtual int? AccessTypeId { get; set; }
        public virtual int Version { get; set; }

        public string DataExtension { get; set; }

        public bool IsEnabled { get; set; }

        public List<DeleteNodeDtoBindingModel> Nodes { get; set; }

        public List<AssetMediaDto> AssetMedia { get; set; }

        public List<DeleteGalleryDtoBindingModel> Galleries { get; set; }

        public AssetDto()
        {
            Nodes = new List<DeleteNodeDtoBindingModel>();
            AssetMedia = new List<AssetMediaDto>();
            Galleries = new List<DeleteGalleryDtoBindingModel>();
        }
    }

    public class AssetSolrDto : AssetDto
    {
        public AssetSolrDto()
        {
            this.Nodes_en = new List<string>();
            this.MediaGalleries = new List<GalleryDto>();
            this.Microsite_Layout = new LayoutDto();
        }

        public int Id { get; set; }

        public string Url { get; set; }

        public string Title_en { get; set; }

        public List<int> Nodes_Id { get; set; }

        public List<string> Nodes_en { get; set; }

        public List<string> Nodes_slug { get; set; }

        public string Description_en { get; set; }

        public DateTime PublicationDate { get; set; }

        public string Content_en { get; set; }

        public string Keywords_en { get; set; }

        public int TemplateId { get; set; }

        public string TemplateClass { get; set; }

        public string Parent { get; set; }
        public MediaSizesPaths MediaSizesPaths { get; set; }
        public string MediaDiscriminator { get; set; }
        public new List<int> Galleries { get; set; }

        public List<GalleryDto> MediaGalleries { get; set; }

        public LayoutDto Microsite_Layout { get; set; }

        public string[] AuthorName { get; set; }
        public int[] AuthorId{ get; set; }
    }

    public class MediaSizesPaths
    {
        public string Size1Path { get; set; }
        public string Size2Path { get; set; }
        public string Size3Path { get; set; }
        public string Size4Path { get; set; }
        public string Size5Path { get; set; }
        public string Size6Path { get; set; }
    }

}
