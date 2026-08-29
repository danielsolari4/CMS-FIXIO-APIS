using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Ray.Dtos
{
    public class MediaDto : BaseDto
    {
        public string SourcePath { get; set; }

        public string Size1Path { get; set; }

        public string Size2Path { get; set; }

        public string Size3Path { get; set; }

        public string Size4Path { get; set; }

        public string Size5Path { get; set; }
        public string Size6Path { get; set; }

        public bool IsEnabled { get; set; }

        public string FileId { get; set; }

        [StringLength(100, ErrorMessage = "Title max length is 100")]
        public virtual string Title { get; set; }

        [StringLength(256, ErrorMessage = "Caption max length is 256")]
        public string Caption { get; set; }

        public string MediaUrl { get; set; }
        public string MediaType { get; set; }

        public string Description { get; set; }

        public string AlternativeText { get; set; }

        public string Keywords { get; set; }

        public int Version { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Metadata { get; set; }

        public long FileSize { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public List<DeleteKeywordDtoBindingModel> KeywordList { get; set; }

        public List<DeleteCategoryDtoBindingModel> Categories { get; set; }

        public List<DeleteGalleryDtoBindingModel> Galleries { get; set; }

        public List<AssetMediaDto> AssetMedia { get; set; }

        public int Order { get; set; }

        public bool Featured { get; set; }

        public int ShareCount { get; set; }

        public int ViewsCount { get; set; }

        public string Discriminator { get; set; }

        public DateTime? PublicationDate { get; set; }

        public MediaSizesPaths SizesPaths { get; set; }
        
        [JsonProperty(PropertyName = "Categories_Name")]
        public string[] CategoriesName { get; set; }

        public MediaDto()
        {
            Galleries = new List<DeleteGalleryDtoBindingModel>();
            Categories = new List<DeleteCategoryDtoBindingModel>();
            KeywordList = new List<DeleteKeywordDtoBindingModel>();
            AssetMedia = new List<AssetMediaDto>();
        }
    }

    public class UpdateMediaDtoBindingModel : MediaDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        public override string Title { get; set; }
    }

    public class DeleteMediaDtoBindingModel : MediaDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
