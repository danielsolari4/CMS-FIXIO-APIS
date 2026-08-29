using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class ComponentInstanceDto : BaseDto
    {
        public override int Id { get; set; }

        public List<DeleteMediaDtoBindingModel> Media { get; set; }

        public List<DeleteGalleryDtoBindingModel> Galleries { get; set; }

        public List<PageDto> Assets { get; set; }

        public int Position { get; set; }

        public string Size { get; set; }

        public ReplacementDataDto ReplacementData { get; set; }

        public ComponentInstanceDto()
        {
            Media = new List<DeleteMediaDtoBindingModel>();
            Galleries = new List<DeleteGalleryDtoBindingModel>();
            Assets = new List<PageDto>();
        }

        [Required]
        public ComponentLayoutDto ComponentLayout { get; set; }
    }
    public class ReplacementDataDto
    {
        public string NodeContentTitle { get; set; }

        public string AssetContentTitle { get; set; }

        public string AssetMediaSizePath1 { get; set; }

        public string AssetMediaSizePath2 { get; set; }

        public string AssetMediaSizePath3 { get; set; }

        public string AssetMediaSizePath4 { get; set; }

        public string AssetMediaSizePath5 { get; set; }

        public string MediaSizePath1 { get; set; }

        public string MediaSizePath2 { get; set; }

        public string MediaSizePath3 { get; set; }

        public string MediaSizePath4 { get; set; }

        public string MediaSizePath5 { get; set; }

        public string MediaTitle { get; set; }

        public int MediaId { get; set; }

        public int AssetId { get; set; }

        public string CreationUser { get; set; }

        public string LastModification { get; set; }
    }
}
