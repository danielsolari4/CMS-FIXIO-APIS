using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public abstract class AssetContentDto : BaseDto
    {
        [Required]
        [StringLength(1000, ErrorMessage = "Title max length is 1000")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "SocialNetworkTitle max length is 1000")]
        public string SocialNetworkTitle { get; set; }

        public string Description { get; set; }

        [StringLength(4000, ErrorMessage = "Introduction max length is 4000")]
        public string Introduction { get; set; }

        [StringLength(1000, ErrorMessage = "Volanta max length is 1000")]
        public string Volanta { get; set; }

        [StringLength(1000, ErrorMessage = "MobileTitle max length is 1000")]
        public string MobileTitle { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LanguageId")]
        public virtual int LanguageId { get; set; }

        public virtual int TemplateId { get; set; }

        public AssetContentDto() { }
    }
}
