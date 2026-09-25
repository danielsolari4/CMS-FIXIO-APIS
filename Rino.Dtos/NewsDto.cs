using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Rino.Dtos
{
    public class NewsDto : PageDto
    {
        public DateTime? ExpirationDate { get; set; }

        public string EditorComments { get; set; }

        public List<AuthorDto> Authors { get; set; }

        public NewsSourceDto NewsSource { get; set; }

        public string NewsSourceNewsId { get; set; }

        public List<NewsDto> RelatedAssets { get; set; }
        public List<AssetJsonDto> AssetJson { get; set; }

        public bool IsDeleted { get; set; }

        public NewsDto()
        {
            Authors = new List<AuthorDto>();
            RelatedAssets = new List<NewsDto>();
            AssetJson = new List<AssetJsonDto>();
        }

        public ICollection<PrintEditionNewDto> PrintEditionNew { get; set; }
    }

    public class CreateNewsDtoBindingModel : NewsDto
    {
        [Required]
        public override List<PageContentDto> Content { get; set; }

        public CreateNewsDtoBindingModel()
        {
            Content = null;
        }
    }

    public class ImportNewsDtoBindingModel : CreateNewsDtoBindingModel
    {
        public string ImportImageUrl { get; set; }
        public string PhotoName { get; set; }
        public List<ImportImage> ImportImages { get; set; }
    }

    public class ImportImage
    {
        public string ImportImageUrl { get; set; }
        public string Description { get; set; }
    }


    public class UpdateNewsDtoBindingModel : NewsDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        public override List<PageContentDto> Content { get; set; }

        public UpdateNewsDtoBindingModel()
        {
            Content = null;
        }
    }

    public class DeleteNewsDtoBindingModel : NewsDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }

    public class RestoreNewsDtoBindingModel : NewsDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }
}
