using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class PageDto : AssetDto
    {
        public bool WasPublished { get; set; }

        public string PublicationUser { get; set; }

        public DateTime? PublicationDate { get; set; }

        public int ViewsCount { get; set; }

        public int? VotesCount { get; set; }

        public  int ShareCount { get; set; }

        public int? CommentsCount { get; set; }

        [StringLength(300, ErrorMessage = "Url max length is 300")]
        public string Url { get; set; }

        [StringLength(100, ErrorMessage = "Url max length is 100")]
        public string ShortUrl { get; set; }

        public virtual List<PageContentDto> Content { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsAlert { get; set; }

        public PageDto()
        {
            Content = new List<PageContentDto>();
        }
    }

    public class CreatePageDtoBindingModel : PageDto
    {
        [Required]
        public override List<PageContentDto> Content { get; set; }

        public CreatePageDtoBindingModel()
        {
            Content = null;
        }
    }

    public class UpdatePageDtoBindingModel : PageDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        public override List<PageContentDto> Content { get; set; }

        public UpdatePageDtoBindingModel()
        {
            Content = null;
        }
    }

    public class DeletePageDtoBindingModel : PageDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
